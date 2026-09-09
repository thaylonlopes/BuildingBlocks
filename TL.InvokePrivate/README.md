# 🔍 TL.InvokePrivate

> Utilitário para **invocação dinâmica de membros privados** via Reflection com desembrulho automático de exceções de negócio (`ExceptionDispatchInfo`), suporte síncrono e assíncrono para .NET 6, .NET 8 e .NET 9.

[![.NET 6.0](https://img.shields.io/badge/.NET-6.0-purple.svg)](https://dotnet.microsoft.com/)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20(LTS)-blue.svg)](https://dotnet.microsoft.com/)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## ⚠️ Nota de Boas Práticas & Arquitetura

> [!CAUTION]
> **PACOTE APOSENTADO / DEPRECATED (`<IsPackable>false</IsPackable>`)**
> O pacote `TL.InvokePrivate` foi formalmente descontinuado a partir da versão 0.2.0 (`US-EVO-008`) e marcado com `[Obsolete]`. A publicação no NuGet foi desativada.
> Para novo desenvolvimento de código de produção e testes modernos, **evite quebrar o encapsulamento** e prefira membros com modificador `internal` combinados com `[InternalsVisibleTo]`.

---

## 📦 Instalação

Adicione o pacote ao seu projeto via .NET CLI:

```bash
dotnet add package TL.InvokePrivate --version 0.1.0
```

Ou através do Gerenciador de Pacotes do Visual Studio:

```powershell
Install-Package TL.InvokePrivate -Version 0.1.0
```

---

## 🚀 Como Usar

### 1. Invocando Métodos Privados com Retorno

```csharp
using TL.InvokePrivate;

var orderCalculator = new OrderCalculator();

// Invoca o método privado "CalculateTax" passando parâmetros
decimal tax = MethodInvoker.InvokePrivateMethod<decimal>(orderCalculator, "CalculateTax", 1000m, "SP");
```

### 2. Invocando Métodos Assíncronos (`async/await`)

```csharp
var processor = new PaymentProcessor();

// Executa e aguarda a conclusão do método privado assíncrono
await MethodInvoker.InvokePrivateMethodAsync(processor, "ProcessInternalPaymentAsync", orderId);
```

### 3. Desembrulho Automático de Exceções

Ao contrário da Reflection padrão que empacota erros em `TargetInvocationException`, o `MethodInvoker` desembrulha e relança diretamente a exceção original de negócio (ex: `InvalidOperationException`, `ArgumentException`), preservando o StackTrace original.

---

## 🏛️ Compatibilidade
- **.NET 6.0**
- **.NET 8.0** (LTS)
- **.NET 9.0** (Standard)
- **Zero Dependências Externas** (100% BCL Pura).
