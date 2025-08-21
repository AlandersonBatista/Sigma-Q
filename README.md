# Sigma-Q

O **Sigma-Q** é uma ferramenta desenvolvida em **VB.NET** para apoiar o controle de qualidade laboratorial e a tomada de decisão técnica em análises químicas.  
Ele automatiza a avaliação estatística de resultados, identifica tendências e desvios, e gera relatórios em PDF para uso direto pelos técnicos responsáveis.

---

## 📌 Objetivo

Auxiliar o técnico que está na frente do equipamento a interpretar resultados de forma rápida e confiável, reduzindo o risco de erros de avaliação e garantindo maior consistência no processo analítico.

---

## ⚙️ Funcionalidades

- 🔎 **Identificação automática** da amostra recebida a partir do resultado do equipamento.  
- 📊 **Busca de histórico** no banco de dados, com comparação aos três últimos resultados.  
- 📈 **Análise de tendência**: detecção de aumento ou redução em relação ao histórico recente.  
- 🧪 **Análise estatística global**: aplicação de distribuição normal (Curva de Gauss) e desvio absoluto mediano (MAD) para detectar valores críticos.  
- 🚨 **Alertas inteligentes**: geração de mensagens como:
  - "Alerta: Valor acima/abaixo da média"
  - "Alerta CRÍTICO: Valor fora de 2×MAD"  
- 📝 **Geração automática de relatórios em PDF** (com lista de desvios observados).  
- 💾 **Armazenamento** do relatório gerado para rastreabilidade.  

---

## 🖥️ Exemplo de Relatório

Relatórios trazem de forma organizada os desvios detectados em cada amostra:

- **Análise de Tendência** (últimos 3 resultados).  
- **Análise Estatística** (Curva de Gauss e MAD).  
- Destaque visual para **alertas críticos**.  

📄 Exemplo real: 
---[RelatorioDesvios_20250801_061848.pdf](https://github.com/user-attachments/files/21909429/RelatorioDesvios_20250801_061848.pdf)

<img width="831" height="866" alt="image" src="https://github.com/user-attachments/assets/9500ca3c-4cc0-4056-9793-6e24210f7e18" />



## 🛠️ Tecnologias

- **Linguagem:** Visual Basic .NET  
- **Framework:** .NET Framework (Windows Forms)  
- **Banco de Dados:** SQLite (embutido, sem necessidade de instalação adicional)  
- **Relatórios:** Geração de PDF automatizada  

---

## 🚀 Instalação e Execução

1. Clone este repositório:
   ```
   git clone https://github.com/AlandersonBatista/Sigma-Q.git
   ```

Abra o projeto no Visual Studio (versão 2019 ou superior).

Compile a solução (Build Solution).

Execute o aplicativo.

## 🧭 Estrutura do Projeto
Formulários/ → Interfaces gráficas (Windows Forms).

Módulos/ → Funções de análise estatística e lógicas auxiliares.

Classes/ → Modelos de dados e regras de negócio.

Relatórios/ → Saída em PDF com alertas gerados.

## 📚 Roadmap (futuro)
 Dashboard visual de resultados.

 Exportação para Excel/Power BI.

 Configuração de limites críticos customizados por tipo de amostra.

## 🤝 Contribuição
Pull requests são bem-vindos!
Para contribuir:

Faça um fork do projeto.

Crie uma branch para sua feature.

Submeta um Pull Request descrevendo sua alteração.

## 📄 Licença
Este projeto está sob a licença MIT – veja o arquivo LICENSE para mais detalhes.

👤 Autor
Alanderson Batista
