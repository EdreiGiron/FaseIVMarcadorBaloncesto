# app.py
import os
import io
import requests
from datetime import datetime
from io import BytesIO

from fastapi import FastAPI, Query, Header, Response, HTTPException
from fastapi.responses import PlainTextResponse

from reportlab.lib.pagesizes import A4, letter
from reportlab.lib import colors
from reportlab.platypus import (
    SimpleDocTemplate, Table, TableStyle, Paragraph, Spacer, Image
)
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.utils import ImageReader

# -----------------------------------------------------------------------------
# Config básica
# -----------------------------------------------------------------------------
app = FastAPI(title="Marcador Reportes", version="1.0.0")

# Bases por defecto pensadas para red interna de Docker.
# Puedes sobreescribirlas en docker-compose con environment:
#   TEAMS_BASE=http://teams-service:8081
#   PLAYERS_BASE=http://players-service:8082
#   MATCHES_BASE=http://matches-service:8084  (si lo tienes en contenedor)
TEAMS_BASE   = os.getenv("TEAMS_BASE",   "http://teams-service:8081")
PLAYERS_BASE = os.getenv("PLAYERS_BASE", "http://players-service:8082")
MATCHES_BASE = os.getenv("MATCHES_BASE", "http://localhost:5130")
BACK_BASE    = os.getenv("BACK_BASE",    MATCHES_BASE)

# Paths o URLs completas (si empiezan con http, se usan tal cual)
EQUIPOS_PATH    = os.getenv("EQUIPOS_PATH", "/api/equipos")
PLAYERS_BY_TEAM = os.getenv("PLAYERS_BY_TEAM", "/api/jugadores")
MATCH_HISTORY   = os.getenv("MATCH_HISTORY", "/api/partidos")


def _url(base: str, path_or_url: str) -> str:
    """Devuelve URL final. Si path_or_url ya es una URL completa, la retorna tal cual;
    si no, concatena base + path."""
    return path_or_url if path_or_url.startswith("http") else f"{base}{path_or_url}"


def _hdr(authorization: str | None):
    return {"Authorization": authorization} if authorization else {}


def _ensure_ok(r: requests.Response, url: str):
    if not r.ok:
        detail = r.text[:300] if r.text else ""
        raise HTTPException(status_code=r.status_code, detail=f"{url} → {r.status_code}: {detail}")


def _pdf_bytes(
    title: str,
    headers: list[list | str],
    rows: list[list[str]],
    subtitle: str | None = None,
) -> bytes:
    buf = io.BytesIO()
    doc = SimpleDocTemplate(
        buf, pagesize=A4,
        leftMargin=36, rightMargin=36, topMargin=42, bottomMargin=42
    )

    styles = getSampleStyleSheet()
    h1 = ParagraphStyle(
        "h1",
        parent=styles["Title"],
        fontName="Helvetica-Bold",
        fontSize=22,
        leading=24,
        spaceAfter=6
    )
    sub = ParagraphStyle(
        "sub",
        parent=styles["Normal"],
        fontSize=10,
        textColor=colors.HexColor("#666"),
        spaceAfter=16,
    )

    story: list = []
    story.append(Paragraph(title, h1))
    story.append(Paragraph("Generado por Marcador de Baloncesto", sub))
    if subtitle:
        story.append(Paragraph(subtitle, sub))

    data = [headers] + rows
    table = Table(data, repeatRows=1)
    table.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#f2f2f2")),
        ("TEXTCOLOR",  (0, 0), (-1, 0), colors.HexColor("#222")),
        ("FONTNAME",   (0, 0), (-1, 0), "Helvetica-Bold"),
        ("FONTSIZE",   (0, 0), (-1, 0), 11),
        ("BOTTOMPADDING", (0,0), (-1,0), 8),

        ("FONTNAME", (0,1), (-1,-1), "Helvetica"),
        ("FONTSIZE", (0,1), (-1,-1), 10),
        ("ROWBACKGROUNDS", (0,1), (-1,-1), [colors.white, colors.HexColor("#fbfbfb")]),
        ("GRID", (0,0), (-1,-1), 0.5, colors.HexColor("#cccccc")),
        ("LEFTPADDING", (0,0), (-1,-1), 6),
        ("RIGHTPADDING",(0,0), (-1,-1), 6),
        ("TOPPADDING",  (0,0), (-1,-1), 4),
        ("BOTTOMPADDING",(0,0), (-1,-1), 4),
    ]))
    story.append(table)
    doc.build(story)
    buf.seek(0)
    return buf.read()

# -----------------------------------------------------------------------------
@app.get("/", response_class=PlainTextResponse)
def root():
    return "Marcador Reportes API – OK"


@app.get("/health", response_class=PlainTextResponse)
def health():
    return "ok"

# -----------------------------------------------------------------------------
# /pdf/equipos
# -----------------------------------------------------------------------------
@app.get("/pdf/equipos")
def pdf_equipos(
    search: str | None = Query(default=None),
    ciudad: str | None = Query(default=None),
    authorization: str | None = Header(default=None),
):
    url = _url(TEAMS_BASE, EQUIPOS_PATH)
    params = {}
    if search: params["search"] = search
    if ciudad: params["ciudad"] = ciudad

    r = requests.get(url, params=params, headers=_hdr(authorization), timeout=30)
    _ensure_ok(r, url)
    equipos = r.json() or []

    # Create PDF with logos
    buf = io.BytesIO()
    doc = SimpleDocTemplate(buf, pagesize=A4, leftMargin=36, rightMargin=36, topMargin=42, bottomMargin=42)

    styles = getSampleStyleSheet()
    h1 = ParagraphStyle("h1", parent=styles["Title"], fontName="Helvetica-Bold", fontSize=22, leading=24, spaceAfter=6)
    sub = ParagraphStyle("sub", parent=styles["Normal"], fontSize=10, textColor=colors.HexColor("#666"), spaceAfter=16)

    story = []
    story.append(Paragraph("Reporte de Equipos Registrados", h1))
    story.append(Paragraph("Generado por MarcadorReportesPDF-Fase3", sub))

    # Table with logos
    headers_tbl = ["Logo", "Id", "Equipo", "Ciudad", "Puntos", "Faltas"]
    data = [headers_tbl]

    for e in equipos:
        logo_cell = ""
        logo_url = e.get("LogoUrl") or e.get("logoUrl")

        team_name = str(e.get("nombre") or e.get("Nombre") or "").lower().replace(" ", "_")

        if logo_url:
            try:
                logo_response = requests.get(logo_url, timeout=5)
                if logo_response.status_code == 200:
                    logo_img = Image(ImageReader(io.BytesIO(logo_response.content)), width=30, height=30)
                    logo_cell = logo_img
                else:
                    logo_cell = "Sin logo"
            except:
                logo_cell = "Sin logo"
        else:
            try:
                assets_path = f"assets/{team_name}.png"
                if os.path.exists(assets_path):
                    logo_img = Image(assets_path, width=30, height=30)
                    logo_cell = logo_img
                else:
                    logo_cell = "Sin logo"
            except:
                logo_cell = "Sin logo"

        data.append([
            logo_cell,
            str(e.get("id") or e.get("Id") or ""),
            str(e.get("nombre") or e.get("Nombre") or ""),
            str(e.get("ciudad") or e.get("Ciudad") or "–"),
            str(e.get("puntos") or e.get("Puntos") or 0),
            str(e.get("faltas") or e.get("Faltas") or 0),
        ])

    table = Table(data, colWidths=[50, 30, 120, 80, 50, 50], repeatRows=1)
    table.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#f2f2f2")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.HexColor("#222")),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("FONTSIZE", (0, 0), (-1, 0), 11),
        ("BOTTOMPADDING", (0, 0), (-1, 0), 8),
        ("FONTNAME", (0, 1), (-1, -1), "Helvetica"),
        ("FONTSIZE", (0, 1), (-1, -1), 10),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#fbfbfb")]),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#cccccc")),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
        ("ALIGN", (0, 0), (-1, -1), "CENTER"),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
    ]))

    story.append(table)
    doc.build(story)
    buf.seek(0)

    return Response(
        content=buf.read(),
        media_type="application/pdf",
        headers={"Content-Disposition": 'attachment; filename="Equipos_Registrados.pdf"'}
    )

# -----------------------------------------------------------------------------
# /pdf/jugadores-por-equipo
# -----------------------------------------------------------------------------
@app.get("/pdf/jugadores-por-equipo")
def pdf_jugadores_por_equipo(
    equipoId: int = Query(..., description="Id del equipo"),
    authorization: str | None = Header(default=None),
):
    # Obtener jugadores
    url = _url(PLAYERS_BASE, PLAYERS_BY_TEAM)
    params = {"equipoId": equipoId}

    r = requests.get(url, params=params, headers=_hdr(authorization), timeout=30)
    _ensure_ok(r, url)
    jugadores = r.json() or []

    if isinstance(jugadores, dict) and 'items' in jugadores:
        jugadores = jugadores['items']

    # Obtener nombre del equipo
    team_url = _url(TEAMS_BASE, f"{EQUIPOS_PATH}/{equipoId}")
    try:
        team_r = requests.get(team_url, headers=_hdr(authorization), timeout=30)
        if team_r.ok:
            team_data = team_r.json()
            team_name = team_data.get("nombre") or team_data.get("Nombre") or f"Equipo {equipoId}"
        else:
            team_name = f"Equipo {equipoId}"
    except:
        team_name = f"Equipo {equipoId}"

    headers_tbl = ["#", "Jugador", "Posición", "Número"]
    rows: list[list[str]] = []
    for i, j in enumerate(jugadores, start=1):
        rows.append([
            str(i),
            str(j.get("nombre") or j.get("name") or ""),
            str(j.get("posicion") or j.get("position") or "–"),
            str(j.get("numero") or j.get("number") or "–"),
        ])

    subtitle = f"Equipo: {team_name}"
    pdf = _pdf_bytes("Reporte de Jugadores por Equipo", headers_tbl, rows, subtitle)
    return Response(
        content=pdf,
        media_type="application/pdf",
        headers={"Content-Disposition": f'attachment; filename="JugadoresXEquipo_{equipoId}.pdf"'}
    )

# -----------------------------------------------------------------------------
# /pdf/historial-partidos
# -----------------------------------------------------------------------------
@app.get("/pdf/historial-partidos")
def pdf_historial_partidos(
    temporadaId: int | None = Query(None),
    authorization: str | None = Header(default=None),
):
    url = _url(MATCHES_BASE, MATCH_HISTORY)

    params = {}
    if temporadaId is not None:
        params["torneo_id"] = temporadaId  # PHP microservice usa torneo_id

    r = requests.get(url, params=params, headers=_hdr(authorization), timeout=30)
    _ensure_ok(r, url)
    response = r.json() or {}
    partidos = response.get('items', []) if isinstance(response, dict) else response or []

    headers_tbl = ["#", "Fecha/Hora", "Local", "Visitante", "Marcador"]
    rows: list[list[str]] = []
    for i, p in enumerate(partidos, start=1):
        if not isinstance(p, dict):
            continue
        local = p.get("EquipoLocalNombre") or f"Equipo {p.get('EquipoLocalId', '?')}"
        vis   = p.get("EquipoVisitanteNombre") or f"Equipo {p.get('EquipoVisitanteId', '?')}"

        ml = p.get("MarcadorLocal") or p.get("PuntosLocal") or 0
        mv = p.get("MarcadorVisitante") or p.get("PuntosVisitante") or 0
        fh = p.get("FechaHora") or ""
        try:
            fh_fmt = datetime.fromisoformat(str(fh).replace("Z","")).strftime("%Y-%m-%d %H:%M")
        except Exception:
            fh_fmt = str(fh)

        rows.append([str(i), fh_fmt, str(local), str(vis), f"{ml} - {mv}"])

    subtitle = f"Temporada: {temporadaId if temporadaId is not None else 'todas'}"
    pdf = _pdf_bytes("Historial de partidos", headers_tbl, rows, subtitle)
    return Response(
        content=pdf,
        media_type="application/pdf",
        headers={"Content-Disposition": 'attachment; filename="Historial_Partidos.pdf"'}
    )

# -----------------------------------------------------------------------------
# /pdf/roster
# -----------------------------------------------------------------------------
@app.get("/pdf/roster")
def pdf_roster(
    partidoId: int = Query(...),
    authorization: str | None = Header(default=None),
):
    url = _url(MATCHES_BASE, f"{MATCH_HISTORY}/{partidoId}/roster")

    r = requests.get(url, headers=_hdr(authorization), timeout=30)
    _ensure_ok(r, url)
    roster = r.json() or []

    headers_tbl = ["#", "EquipoId", "Jugador", "Posición"]
    rows: list[list[str]] = []
    for i, item in enumerate(roster, start=1):
        rows.append([
            str(i),
            str(item.get("equipo_id") or item.get("equipoId") or ""),
            str(item.get("jugador_nombre") or item.get("jugadorNombre") or ""),
            str(item.get("posicion") or item.get("position") or "–"),
        ])

    pdf = _pdf_bytes(f"Roster – Partido {partidoId}", headers_tbl, rows)
    return Response(
        content=pdf,
        media_type="application/pdf",
        headers={"Content-Disposition": f'attachment; filename="Roster_Partido_{partidoId}.pdf"'}
    )

# -----------------------------------------------------------------------------
# /pdf/scouting (opcional; si no tiene estadísticas, imprime ceros)
# -----------------------------------------------------------------------------
@app.get("/pdf/scouting")
def pdf_scouting(
    jugadorId: int = Query(...),
    authorization: str | None = Header(default=None),
):
    # Intento 1: /api/jugadores/{id}
    jurl = _url(PLAYERS_BASE, f"/api/jugadores/{jugadorId}")
    rj = requests.get(jurl, headers=_hdr(authorization), timeout=30)
    if not rj.ok:
        # Fallback: /api/players/{id} (compatibilidad)
        jurl_alt = _url(PLAYERS_BASE, f"/api/players/{jugadorId}")
        rj = requests.get(jurl_alt, headers=_hdr(authorization), timeout=30)
        _ensure_ok(rj, jurl_alt)
    else:
        _ensure_ok(rj, jurl)
    j = rj.json() or {}

    nombre   = str(j.get("nombre") or j.get("name") or "")
    posicion = str(j.get("posicion") or j.get("position") or "–")
    numero   = str(j.get("numero") or j.get("number") or "–")
    equipo   = str(j.get("equipoNombre") or j.get("team_name") or f"Equipo {j.get('equipoId', '?')}")
    puntos_totales = j.get("puntos") or 0
    faltas_totales = j.get("faltas") or 0

    # Obtener partidos del equipo para calcular estadísticas básicas
    try:
        partidos_url = _url(MATCHES_BASE, MATCH_HISTORY)
        partidos_params = {"equipo_id": j.get("equipoId") or j.get("team_id"), "estado": "Finalizado"}
        partidos_r = requests.get(partidos_url, params=partidos_params, headers=_hdr(authorization), timeout=30)
        partidos_jugados = len(partidos_r.json()) if partidos_r.ok else 1
    except:
        partidos_jugados = 1

    # Calcular promedios básicos
    ppg = round(puntos_totales / max(partidos_jugados, 1), 1)
    fpg = round(faltas_totales / max(partidos_jugados, 1), 1)

    # Tabla simplificada con datos disponibles
    headers_tbl = ["Puntos Totales", "Faltas Totales", "Partidos Jugados", "PPG", "FPG"]
    rows = [[str(puntos_totales), str(faltas_totales), str(partidos_jugados), str(ppg), str(fpg)]]

    subtitle = f"{nombre} • #{numero} • {posicion} • {equipo}"
    pdf = _pdf_bytes("Estadísticas del Jugador", headers_tbl, rows, subtitle)
    return Response(
        content=pdf,
        media_type="application/pdf",
        headers={"Content-Disposition": f'attachment; filename="estadisticas_jugador_{jugadorId}.pdf"'}
    )

# -----------------------------------------------------------------------------
# /pdf/lideres?metric=puntos
# -----------------------------------------------------------------------------
@app.get("/pdf/lideres")
def pdf_lideres(
    metric: str = Query(default="puntos", regex="^(puntos|faltas)$"),
    equipoId: int | None = Query(default=None),
    authorization: str | None = Header(default=None)
):
    base = BACK_BASE
    url_l = _url(base, "/api/estadisticas/lideres")
    params = {"metric": metric}
    if equipoId: params["equipoId"] = equipoId

    try:
        r = requests.get(url_l, params=params, headers=_hdr(authorization), timeout=20)
        if r.status_code == 200:
            data = r.json()
        else:
            raise Exception("no leaders endpoint")
    except Exception:
        # Fallback: construir ranking desde /api/jugadores
        params_j = {}
        if equipoId: params_j["equipoId"] = equipoId
        r2 = requests.get(_url(base, "/api/jugadores"), params=params_j, headers=_hdr(authorization), timeout=20)
        _ensure_ok(r2, "/api/jugadores")
        players = r2.json()
        field = "puntos" if metric == "puntos" else "faltas"

        norm = []
        for j in players:
            norm.append({
                "nombre": j.get("nombre") or j.get("Nombre"),
                "equipoNombre": j.get("equipoNombre") or j.get("EquipoNombre") or "",
                "posicion": j.get("posicion") or j.get("Posicion") or "—",
                "valor": j.get(field) or j.get(field.capitalize()) or 0,
            })
        data = sorted(norm, key=lambda x: x["valor"], reverse=True)

    top3 = data[:3]
    resto = data[3:13]

    # ===== PDF =====
    buf = BytesIO()
    doc = SimpleDocTemplate(buf, pagesize=letter, leftMargin=36, rightMargin=36, topMargin=42, bottomMargin=36)
    st = getSampleStyleSheet()
    H1 = st['Title']; H1.fontSize = 24
    P  = st['Normal']; P.leading = 14

    story = []
    title = f"Líderes – { 'Puntos' if metric=='puntos' else 'Faltas personales' }"
    if equipoId: title += f" (Equipo {equipoId})"
    story += [Paragraph(title, H1), Spacer(1, 8), Paragraph("Generado por MarcadorReportesPDF", st['Italic']), Spacer(1, 14)]

    if top3:
        tdata = [["#", "Jugador", "Equipo", "Posición", "Valor"]]
        for i, p in enumerate(top3, start=1):
            tdata.append([i, p.get("nombre",""), p.get("equipoNombre",""), p.get("posicion","—"), p.get("valor",0)])
        t = Table(tdata, colWidths=[24, 200, 150, 80, 60])
        t.setStyle(TableStyle([
            ('BACKGROUND', (0,0), (-1,0), colors.HexColor("#0f172a")),
            ('TEXTCOLOR', (0,0), (-1,0), colors.white),
            ('FONTNAME', (0,0), (-1,0), 'Helvetica-Bold'),
            ('ALIGN', (0,0), (-1,-1), 'CENTER'),
            ('ALIGN', (1,1), (3,-1), 'LEFT'),
            ('INNERGRID', (0,0), (-1,-1), 0.5, colors.grey),
            ('BOX', (0,0), (-1,-1), 0.75, colors.grey),
            ('FONTSIZE', (0,0), (-1,0), 12),
        ]))
        story += [Paragraph("<b>Top 3</b>", st['Heading2']), Spacer(1,6), t, Spacer(1,14)]

    if resto:
        rdata = [["#", "Jugador", "Equipo", "Posición", "Valor"]]
        for i, p in enumerate(resto, start=4):
            rdata.append([i, p.get("nombre",""), p.get("equipoNombre",""), p.get("posicion","—"), p.get("valor",0)])
        rt = Table(rdata, colWidths=[24, 200, 150, 80, 60])
        rt.setStyle(TableStyle([
            ('BACKGROUND', (0,0), (-1,0), colors.HexColor("#f8fafc")),
            ('FONTNAME', (0,0), (-1,0), 'Helvetica-Bold'),
            ('ALIGN', (0,0), (-1,-1), 'CENTER'),
            ('ALIGN', (1,1), (3,-1), 'LEFT'),
            ('INNERGRID', (0,0), (-1,-1), 0.25, colors.lightgrey),
            ('BOX', (0,0), (-1,-1), 0.75, colors.lightgrey),
        ]))
        story += [Paragraph("<b>Ranking</b>", st['Heading2']), Spacer(1,6), rt]

    doc.build(story)
    buf.seek(0)
    return Response(buf.getvalue(), media_type="application/pdf",
        headers={"Content-Disposition": 'attachment; filename="lideres.pdf"'}
    )

# -----------------------------------------------------------------------------
# Main
# -----------------------------------------------------------------------------
if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app:app", host="0.0.0.0", port=int(os.getenv("PORT", "5055")), reload=True)
