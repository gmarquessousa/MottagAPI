"""Full integration demo & smoke test for mottag-api.

Requisitos:
    pip install requests tabulate (tabulate opcional para tabela mais bonita)

O script executa:
  0. Limpeza: remove todos os registros (Tags -> Motos -> Patios) para iniciar ambiente limpo.
  1. CRUD Patio: create, get, update, list, delete, list (espera vazio)
  2. CRUD Moto: create (vinculado ao patio), get, update, list filtrada, delete
  3. CRUD Tag: create, get, update, tentativa duplicada (espera 409), list, delete
  4. Fluxo encadeado: cria patio -> moto -> tag; depois remove na ordem tag -> moto -> patio
  5. Relatório final: métricas de requisições (latência média, sucesso/falha por verbo, status codes)

Observações:
 - O script assume que a API está rodando em BASE_URL.
 - Caso use autenticação futuramente, adapte HEADERS.
 - Em caso de falha crítica, o script continua acumulando resultados para análise final.

Execução:
    python scripts/demo_flow.py

Configurar BASE_URL via env (opcional):
    set MOTTAG_BASE_URL=http://localhost:5210

"""
from __future__ import annotations
import json
import os
import sys
import time
from dataclasses import dataclass, field
from statistics import mean
from typing import Any, Dict, List, Optional, Tuple

import requests

BASE_URL = os.getenv("MOTTAG_BASE_URL", "http://localhost:5210")
HEADERS = {"Content-Type": "application/json"}

###############################################################################
# Data Structures
###############################################################################

@dataclass
class ApiResponse:
    ok: bool
    status: int
    data: Any | None
    error: str | None = None
    elapsed_ms: float = 0.0
    path: str = ""
    method: str = ""


@dataclass
class StatAccumulator:
    entries: List[ApiResponse] = field(default_factory=list)

    def add(self, resp: ApiResponse):
        self.entries.append(resp)

    def summary(self) -> Dict[str, Any]:
        if not self.entries:
            return {"total": 0}
        total = len(self.entries)
        successes = sum(1 for e in self.entries if e.ok)
        failures = total - successes
        by_method: Dict[str, List[ApiResponse]] = {}
        for e in self.entries:
            by_method.setdefault(e.method, []).append(e)
        method_stats = {}
        for m, arr in by_method.items():
            method_stats[m] = {
                "count": len(arr),
                "success": sum(1 for e in arr if e.ok),
                "fail": sum(1 for e in arr if not e.ok),
                "avg_ms": round(mean(x.elapsed_ms for x in arr), 2),
                "p95_ms": round(sorted(x.elapsed_ms for x in arr)[max(0, int(len(arr)*0.95)-1)], 2),
            }
        status_codes: Dict[int, int] = {}
        for e in self.entries:
            status_codes[e.status] = status_codes.get(e.status, 0) + 1
        return {
            "total": total,
            "success": successes,
            "fail": failures,
            "success_pct": round(successes/total*100, 2),
            "avg_ms": round(mean(e.elapsed_ms for e in self.entries), 2),
            "method_stats": method_stats,
            "status_distribution": status_codes,
        }

stats = StatAccumulator()

###############################################################################
# HTTP Helpers
###############################################################################


def _request(method: str, path: str, *, payload: Dict[str, Any] | None = None, params: Dict[str, Any] | None = None) -> ApiResponse:
    url = f"{BASE_URL}{path}"
    start = time.perf_counter()
    try:
        r = requests.request(method, url, headers=HEADERS,
                              data=json.dumps(payload) if payload is not None else None,
                              params=params, timeout=15)
        elapsed = (time.perf_counter() - start) * 1000
        if r.headers.get("Content-Type", "").startswith("application/json"):
            body = r.json()
        else:
            body = r.text
        resp = ApiResponse(r.ok, r.status_code, body if r.ok else None, None if r.ok else str(body), elapsed, path, method)
    except Exception as ex:  # noqa: BLE001
        elapsed = (time.perf_counter() - start) * 1000
        resp = ApiResponse(False, 0, None, str(ex), elapsed, path, method)
    stats.add(resp)
    return resp


def post(path: str, payload: Dict[str, Any]) -> ApiResponse:
    return _request("POST", path, payload=payload)


def get(path: str, params: Dict[str, Any] | None = None) -> ApiResponse:
    return _request("GET", path, params=params)


def put(path: str, payload: Dict[str, Any]) -> ApiResponse:
    return _request("PUT", path, payload=payload)


def delete(path: str) -> ApiResponse:
    return _request("DELETE", path)


def pretty(title: str, resp: ApiResponse):
    print(f"\n=== {title} ===")
    print(f"{resp.method} {resp.path} -> Status: {resp.status} | Ok: {resp.ok} | {resp.elapsed_ms:.1f} ms")
    if resp.ok:
        try:
            print(json.dumps(resp.data, indent=2, ensure_ascii=False))
        except Exception:  # noqa: BLE001
            print(resp.data)
    else:
        print(f"Error: {resp.error}")


###############################################################################
# Utility parse helpers
###############################################################################

def extract_id(api_resp: ApiResponse) -> Optional[str]:
    if not api_resp.ok or not isinstance(api_resp.data, dict):
        return None
    if "data" in api_resp.data and isinstance(api_resp.data["data"], dict):
        return api_resp.data["data"].get("id")
    return api_resp.data.get("id")


###############################################################################
# Cleanup (truncate semantic via DELETE)
###############################################################################

def cleanup_all():
    print("\n--- Limpando dados existentes ---")
    # Ordem: Tags (dependem de Motos) -> Motos (dependem de Patios) -> Patios
    # List endpoints retornam objeto paginado; iteramos até esgotar.
    def list_and_collect(path: str, key: str) -> List[str]:
        collected: List[str] = []
        page = 1
        while True:
            resp = get(f"/api/v1/{path}?page={page}&pageSize=50")
            if not resp.ok:
                break
            if not isinstance(resp.data, dict):
                break
            items = resp.data.get("items") or resp.data.get("data")
            # Alguns retornos de paginação parecem usar 'items' dentro do PagedResult
            if isinstance(items, list):
                for it in items:
                    if isinstance(it, dict) and it.get("id"):
                        collected.append(it["id"])
            total = resp.data.get("total") or 0
            page_count = resp.data.get("pageCount") or 1
            if page >= page_count:
                break
            page += 1
        return collected

    for resource in ("tags", "motos", "patios"):
        ids = list_and_collect(resource, "id")
        if not ids:
            print(f"Nenhum registro em {resource}.")
            continue
        for rid in ids:
            del_resp = delete(f"/api/v1/{resource}/{rid}")
            status = "OK" if del_resp.ok else f"FAIL({del_resp.status})"
            print(f"DELETE {resource}/{rid}: {status}")


###############################################################################
# CRUD Flows
###############################################################################

def crud_patio() -> Tuple[Optional[str], bool]:
    print("\n--- CRUD Patio ---")
    create_resp = post("/api/v1/patios", {
        "nome": "Patio Fluxo",
        "cidade": "Campinas",
        "estado": "SP",
        "pais": "BR",
        "areaM2": 500
    })
    pretty("Create Patio", create_resp)
    pid = extract_id(create_resp)
    if not pid:
        return None, False

    get_resp = get(f"/api/v1/patios/{pid}")
    pretty("Get Patio", get_resp)

    update_resp = put(f"/api/v1/patios/{pid}", {
        "nome": "Patio Fluxo Atualizado",
        "cidade": "Campinas",
        "estado": "SP",
        "pais": "BR",
        "areaM2": 750
    })
    pretty("Update Patio", update_resp)

    list_resp = get("/api/v1/patios?page=1&pageSize=5")
    pretty("List Patios", list_resp)

    delete_resp = delete(f"/api/v1/patios/{pid}")
    pretty("Delete Patio", delete_resp)

    list_after = get("/api/v1/patios?page=1&pageSize=5")
    pretty("List Patios After Delete", list_after)
    return pid, all(r.ok for r in [create_resp, get_resp, update_resp, delete_resp])


def crud_sequence_with_dependencies():
    print("\n--- CRUD Encadeado (Patio -> Moto -> Tag) ---")
    patio_resp = post("/api/v1/patios", {
        "nome": "Patio Encadeado",
        "cidade": "São Paulo",
        "estado": "SP",
        "pais": "BR",
        "areaM2": 999
    })
    pretty("Create Patio", patio_resp)
    pid = extract_id(patio_resp)
    if not pid:
        return None, None, None

    moto_resp = post("/api/v1/motos", {
        "patioId": pid,
        "placa": "XYZ1A23",
        "modelo": "CG 160",
        "status": 0
    })
    pretty("Create Moto", moto_resp)
    mid = extract_id(moto_resp)

    tag_resp = post("/api/v1/tags", {
        "serial": "TAG-INT-1",
        "tipo": 0,
        "bateriaPct": 80
    })
    pretty("Create Tag", tag_resp)
    tid = extract_id(tag_resp)

    # Update Moto
    if mid:
        moto_upd = put(f"/api/v1/motos/{mid}", {
            "patioId": pid,
            "placa": "XYZ1A23",
            "modelo": "CG 160 Premium",
            "status": 0
        })
        pretty("Update Moto", moto_upd)

    # Dup Tag (fail expected 409)
    dup_tag = post("/api/v1/tags", {
        "serial": "TAG-INT-1",
        "tipo": 0,
        "bateriaPct": 85
    })
    pretty("Duplicate Tag Expect 409", dup_tag)

    # Cleanup reverse order
    if tid:
        pretty("Delete Tag", delete(f"/api/v1/tags/{tid}"))
    if mid:
        pretty("Delete Moto", delete(f"/api/v1/motos/{mid}"))
    if pid:
        pretty("Delete Patio", delete(f"/api/v1/patios/{pid}"))
    return pid, mid, tid


def crud_moto_single(patio_id: str) -> Tuple[Optional[str], bool]:
    print("\n--- CRUD Moto (single) ---")
    create_resp = post("/api/v1/motos", {
        "patioId": patio_id,
        "placa": "AAA1B23",
        "modelo": "CB 300",
        "status": 0
    })
    pretty("Create Moto", create_resp)
    mid = extract_id(create_resp)
    if not mid:
        return None, False
    get_resp = get(f"/api/v1/motos/{mid}")
    pretty("Get Moto", get_resp)
    upd_resp = put(f"/api/v1/motos/{mid}", {
        "patioId": patio_id,
        "placa": "AAA1B23",
        "modelo": "CB 300F",
        "status": 0
    })
    pretty("Update Moto", upd_resp)
    list_resp = get(f"/api/v1/motos?patioId={patio_id}&page=1&pageSize=5")
    pretty("List Motos", list_resp)
    del_resp = delete(f"/api/v1/motos/{mid}")
    pretty("Delete Moto", del_resp)
    return mid, all(r.ok for r in [create_resp, get_resp, upd_resp, del_resp])


def crud_tag_single() -> Tuple[Optional[str], bool]:
    print("\n--- CRUD Tag (single) ---")
    create_resp = post("/api/v1/tags", {
        "serial": "TAG-SINGLE-1",
        "tipo": 0,
        "bateriaPct": 70
    })
    pretty("Create Tag", create_resp)
    tid = extract_id(create_resp)
    if not tid:
        return None, False
    get_resp = get(f"/api/v1/tags/{tid}")
    pretty("Get Tag", get_resp)
    upd_resp = put(f"/api/v1/tags/{tid}", {
        "serial": "TAG-SINGLE-1",
        "tipo": 0,
        "bateriaPct": 65
    })
    pretty("Update Tag", upd_resp)
    list_resp = get("/api/v1/tags?page=1&pageSize=5")
    pretty("List Tags", list_resp)
    del_resp = delete(f"/api/v1/tags/{tid}")
    pretty("Delete Tag", del_resp)
    return tid, all(r.ok for r in [create_resp, get_resp, upd_resp, del_resp])


###############################################################################
# Final Report
###############################################################################

def report():
    print("\n================= RELATÓRIO FINAL =================")
    s = stats.summary()
    print(json.dumps(s, indent=2, ensure_ascii=False))
    try:
        from tabulate import tabulate  # type: ignore
        rows = []
        for e in stats.entries:
            rows.append([e.method, e.path, e.status, "OK" if e.ok else "FAIL", f"{e.elapsed_ms:.1f}"])
        print("\nDetalhe das requisições:")
        print(tabulate(rows, headers=["Method", "Path", "Status", "Result", "ms"]))
    except Exception:  # noqa: BLE001
        pass


###############################################################################
# Main
###############################################################################


def main() -> int:
    print(f"BASE_URL = {BASE_URL}")
    cleanup_all()

    # CRUD isolados
    patio_id, patio_ok = crud_patio()
    # Para testar Moto isolada precisamos de um patio novamente
    patio2_resp = post("/api/v1/patios", {
        "nome": "Patio Para Moto",
        "cidade": "Curitiba",
        "estado": "PR",
        "pais": "BR",
        "areaM2": 333
    })
    pretty("Create Patio for Moto CRUD", patio2_resp)
    patio2_id = extract_id(patio2_resp)
    moto_ok = False
    if patio2_id:
        _, moto_ok = crud_moto_single(patio2_id)
        delete(f"/api/v1/patios/{patio2_id}")  # limpeza
    tag_id, tag_ok = crud_tag_single()
    if tag_id:
        delete(f"/api/v1/tags/{tag_id}")

    crud_sequence_with_dependencies()

    report()
    all_ok = patio_ok and moto_ok and tag_ok
    return 0 if all_ok else 1


if __name__ == "__main__":
    sys.exit(main())
