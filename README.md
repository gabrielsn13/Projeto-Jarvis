# Projeto Jarvis

Guia oficial de desenvolvimento incremental do assistente pessoal de IA JARVIS.

## Status atual da implementação

- ✅ Fase 0 (fundação) iniciada
- ✅ MVP inicial da Fase 1 implementado (chat local via console)
- ⏳ Próximo passo: UI WPF mínima e evolução dos módulos de voz

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

## Objetivos do produto (roadmap)

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

## Cronograma inicial (12 semanas)

### Fase 0 (Semana 1) — Fundação

- Estrutura da solução .NET 8, camadas e DI
- Configuração central via `appsettings.json` e logging
- Persistência SQLite para histórico simples

### Fase 1 (Semanas 2–3) — Conversação com IA

- Integração com LLM local via Ollama
- Serviço de chat com contexto curto
- Interface mínima para troca de mensagens

### Fase 2 (Semanas 4–5) — Comandos no computador

- Catálogo inicial de comandos seguros
- Fluxo de autorização/validação
- Logs e tratamento de falhas

### Fase 3 (Semanas 6–7) — Reconhecimento e síntese de voz

- STT (fala para texto)
- TTS (texto para fala)
- Modo alternável texto/voz

### Fase 4 (Semana 8) — Hotword "Jarvis"

- Escuta contínua controlada
- Ativação por palavra-chave

### Fase 5 (Semanas 9–10) — Memória

- Memória de sessão e preferências básicas
- Regras de retenção e privacidade local

### Fase 6 (Semanas 11–12) — Automação inicial e hardening

- Primeiras automações úteis no Windows
- Revisão de segurança e confiabilidade
- Preparação para integrações de API

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