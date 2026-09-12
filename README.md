# Projeto Jarvis

Assistente pessoal local em .NET 8, com foco em segurança, previsibilidade e evolução incremental.

## Status atual da implementação

- ✅ Fase A (fundação + conversação base) concluída
- ✅ Fase B (comandos locais seguros) concluída
- ✅ Fase C (memória de sessão e contexto curto) concluída
- 🔄 Fase D (voz STT/TTS sem hotword) em andamento

## Stack inicial

- C#
- .NET 8
- SQLite
- Ollama
- Modelos LLM locais
- Speech-to-Text (STT)
- Text-to-Speech (TTS)
- WPF

## Evoluções futuras (não implementar agora sem necessidade)

- Unity para interface 3D
- Visão computacional
- Realidade aumentada

## Objetivos do produto (roadmap macro)

1. Conversação com IA
2. Execução de comandos no computador
3. Reconhecimento de voz
4. Síntese de voz
5. Hotword "Jarvis"
6. Memória
7. Automação do Windows
8. Integração com APIs
9. Visão computacional
10. Interface visual futurista
11. Avatar 3D
12. Realidade aumentada

## Regras de engenharia

Priorizar sempre:

- Código simples
- Arquitetura limpa
- Baixo acoplamento
- SOLID
- Dependency Injection
- Interfaces apenas quando houver necessidade real
- Testabilidade
- Segurança
- Código assíncrono quando apropriado
- Tratamento adequado de erros
- Logs
- Configuração via `appsettings.json`
- Separação clara de responsabilidades

Evitar:

- Overengineering
- Abstrações desnecessárias
- Dependências sem necessidade
- Funcionalidades fora da etapa atual
- Alterações arquiteturais grandes sem justificativa
- Lógica de negócio diretamente na UI

## Fluxo obrigatório para novas funcionalidades

Antes de implementar uma funcionalidade grande:

1. Explicar o que será construído
2. Explicar onde ficará na arquitetura
3. Listar arquivos que serão criados/modificados
4. Explicar dependências necessárias
5. Implementar a menor versão funcional
6. Executar (ou sugerir) testes
7. Verificar funcionamento
8. Só então propor melhorias

## Comunicação técnica

Ao usar os termos abaixo, explicar brevemente o conceito antes de aplicar:

- LLM
- embeddings
- RAG
- tool calling
- agents
- vector database
- inference
- STT
- TTS
- context window

## Estratégia de execução

- Evoluir o projeto incrementalmente
- Seguir o roadmap do projeto
- Não implementar funcionalidades futuras sem necessidade da etapa atual

## Roadmap reajustado (v2) — 12 semanas

### Fase A (Semanas 1–2) — Fundação + Conversação base ✅

- Estrutura .NET 8 em camadas + DI
- Configuração central (`appsettings`) + logging
- Integração Ollama (chat texto)
- Interface mínima de conversa
- Contexto curto inicial (quando aplicável)

**Saída esperada:** Jarvis funcional em texto, arquitetura limpa.

### Fase B (Semanas 3–4) — Comandos seguros no Windows ✅

- Catálogo inicial de comandos permitidos
- Prefixo explícito `/cmd <comando>`
- Roteamento determinístico (chat vs comando)
- Bloqueio de comando inválido (não encaminhar ao LLM)
- Logs estruturados + testes unitários

**Saída esperada:** execução local segura e previsível.

### Fase C (Semanas 5–6) — Memória curta e sessões 🔄 (em andamento)

- Memória por `sessionId`
- Janela de contexto (últimas N mensagens)
- Truncamento de mensagens longas
- Comando `/new` para resetar sessão
- Regras iniciais de retenção local (TTL simples)
- Testes de regressão de contexto

**Saída esperada:** conversa mais coerente e controlada.

### Fase D (Semanas 7–8) — Voz (STT/TTS) sem hotword

- STT (fala → texto) com push-to-talk
- TTS (texto → fala) configurável
- Modo alternável texto/voz
- Tratamento de erros de microfone/dispositivo
- Logs de pipeline de áudio

**Saída esperada:** experiência multimodal estável, sem escuta contínua.

### Fase E (Semana 9) — Hotword "Jarvis"

- Escuta contínua controlada (opt-in)
- Ativação por palavra-chave
- Cooldown, sensibilidade e mitigação de falsos positivos
- Indicadores claros de estado (ouvindo/idle)

**Saída esperada:** ativação por voz com segurança operacional.

### Fase F (Semanas 10–11) — Automações úteis + hardening

- 3–5 automações úteis no Windows (com allowlist)
- Timeouts, retry e circuit breaker onde fizer sentido
- Telemetria local mínima (sem dados sensíveis)
- Revisão de segurança (injeção de prompt/comando)

**Saída esperada:** confiabilidade para uso diário.

### Fase G (Semana 12) — Fechamento e preparação de integrações

- Checklist de qualidade final
- Documentação de operação e troubleshooting
- Contratos de integração (APIs futuras)
- Backlog priorizado da vNext

**Saída esperada:** release estável e plano claro de evolução.

## Gate de qualidade por fase

Avançar de fase somente com:

- `dotnet build` passando
- Testes da fase passando
- Demo curta da fase validada
- Checklist de segurança da fase concluído

## Como executar o MVP atual

### Pré-requisitos

- .NET SDK 8+
- Ollama em execução local (`http://localhost:11434`)
- Modelo local baixado no Ollama (exemplo: `llama3.2`)

### Executar

```bash
dotnet build /home/runner/work/Projeto-Jarvis/Projeto-Jarvis/Jarvis.slnx
dotnet run --project /home/runner/work/Projeto-Jarvis/Projeto-Jarvis/src/Jarvis.App/Jarvis.App.csproj
```

Digite mensagens no terminal. Para sair, use `sair`.

Comandos multimodais (Fase D):

- `/voice on` ativa modo voz
- `/voice off` desativa modo voz
- `/voice` alterna o modo voz
- `/ptt` inicia captura push-to-talk (entrada explícita)
- `/tts on` ativa síntese de voz
- `/tts off` desativa síntese de voz
- `/new` inicia nova sessão sem perder o funcionamento do restante do fluxo

### Comandos locais permitidos (Parte 2)

Use o formato `/cmd <identificador>`:

- `/cmd abrir_notepad`
- `/cmd abrir_calculadora`
- `/cmd mostrar_data_hora`

Exemplo de comando bloqueado (fora da allowlist):

- `/cmd abrir_cmd`
