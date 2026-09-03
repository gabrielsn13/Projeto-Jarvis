# Projeto Jarvis

Guia oficial de desenvolvimento incremental do assistente pessoal de IA JARVIS.

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