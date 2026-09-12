#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Contrato esperado pelo C# (WhisperSpeechToTextService):
- stdout: APENAS a transcrição final (texto limpo), sem logs extras
- stderr: mensagens de erro/diagnóstico
- exit code:
    0 = sucesso (com texto)
    2 = uso inválido / argumentos ausentes
    3 = arquivo de áudio não encontrado
    4 = falha ao carregar modelo
    5 = falha na transcrição
    6 = transcrição vazia (nenhuma fala reconhecida)
"""

import argparse
import os
import sys

# Evita unicode quebrado em alguns terminais Windows
try:
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
except Exception:
    pass


def eprint(msg: str) -> None:
    print(msg, file=sys.stderr, flush=True)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Transcreve áudio com faster-whisper e escreve APENAS o texto no stdout."
    )
    parser.add_argument("audio_path", help="Caminho do arquivo de áudio (ex.: .wav)")
    parser.add_argument("--model", default="small", help="Modelo Whisper (tiny/base/small/medium/large-v3...)")
    parser.add_argument("--language", default="pt", help="Idioma (ex.: pt, en, es).")
    parser.add_argument(
        "--device",
        default="auto",
        choices=["auto", "cpu", "cuda"],
        help="Dispositivo de inferência."
    )
    parser.add_argument(
        "--compute_type",
        default=None,
        help="Compute type (ex.: int8, int8_float16, float16, float32). Se omitido, escolhe por device."
    )
    return parser.parse_args()


def resolve_compute_type(device: str, compute_type: str | None) -> str:
    if compute_type:
        return compute_type
    # defaults seguros
    if device == "cpu":
        return "int8"
    if device == "cuda":
        return "float16"
    # auto
    return "int8"


def main() -> int:
    try:
        args = parse_args()
    except SystemExit:
        # argparse já escreveu no stderr
        return 2

    audio_path = args.audio_path

    if not audio_path:
        eprint("audio_path não informado.")
        return 2

    if not os.path.isfile(audio_path):
        eprint(f"Arquivo de áudio não encontrado: {audio_path}")
        return 3

    try:
        from faster_whisper import WhisperModel
    except Exception as ex:
        eprint(f"Falha ao importar faster_whisper: {ex}")
        return 4

    device = args.device
    if device == "auto":
        # fallback simples: CPU
        # (faster-whisper lida com auto internamente em cenários, mas manter explícito evita surpresas)
        device = "cpu"

    compute_type = resolve_compute_type(device, args.compute_type)

    try:
        model = WhisperModel(
            args.model,
            device=device,
            compute_type=compute_type
        )
    except Exception as ex:
        eprint(f"Falha ao carregar modelo '{args.model}' (device={device}, compute_type={compute_type}): {ex}")
        return 4

    try:
        segments, info = model.transcribe(
            audio_path,
            language=args.language,
            vad_filter=True,              # reduz silêncio/ruído
            beam_size=5
        )

        parts: list[str] = []
        for seg in segments:
            txt = (seg.text or "").strip()
            if txt:
                parts.append(txt)

        final_text = " ".join(parts).strip()

        if not final_text:
            eprint("Transcrição vazia (nenhuma fala reconhecida).")
            return 6

        # CONTRATO: stdout somente com texto limpo
        print(final_text, end="", flush=True)
        return 0

    except Exception as ex:
        eprint(f"Falha na transcrição: {ex}")
        return 5


if __name__ == "__main__":
    code = main()
    raise SystemExit(code)