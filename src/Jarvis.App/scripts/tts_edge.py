#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Contrato:
- stdout (sucesso): caminho absoluto do arquivo de áudio gerado
- stderr: mensagens de erro
- exit code:
    0 = sucesso
    2 = uso inválido (args)
    3 = texto vazio
    4 = falha edge-tts
    5 = falha ao salvar arquivo
"""

import argparse
import asyncio
import os
import sys
import tempfile

def parse_args():
    p = argparse.ArgumentParser()
    p.add_argument("--text", required=True)
    p.add_argument("--voice", default="pt-BR-AntonioNeural")
    p.add_argument("--rate", default="+0%")
    p.add_argument("--volume", default="+0%")
    p.add_argument("--pitch", default="+0Hz")
    p.add_argument("--out", default="")
    return p.parse_args()

async def main():
    args = parse_args()

    text = (args.text or "").strip()
    if not text:
        print("Texto vazio.", file=sys.stderr)
        return 3

    try:
        import edge_tts
    except Exception as ex:
        print(f"Falha ao importar edge_tts: {ex}", file=sys.stderr)
        return 4

    out_path = args.out.strip()
    if not out_path:
        fd, temp_path = tempfile.mkstemp(prefix="jarvis-tts-", suffix=".mp3")
        os.close(fd)
        out_path = temp_path

    try:
        communicate = edge_tts.Communicate(
            text=text,
            voice=args.voice,
            rate=args.rate,
            volume=args.volume,
            pitch=args.pitch
        )
        await communicate.save(out_path)
    except Exception as ex:
        print(f"Falha no edge-tts: {ex}", file=sys.stderr)
        return 4

    if not os.path.isfile(out_path) or os.path.getsize(out_path) == 0:
        print("Arquivo de áudio inválido.", file=sys.stderr)
        return 5

    print(os.path.abspath(out_path), end="")
    return 0

if __name__ == "__main__":
    try:
        code = asyncio.run(main())
    except Exception as ex:
        print(f"Erro inesperado: {ex}", file=sys.stderr)
        code = 5
    raise SystemExit(code)