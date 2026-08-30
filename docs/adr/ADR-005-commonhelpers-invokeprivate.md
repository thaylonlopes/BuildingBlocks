# ADR-005: Decisões Arquiteturais do Pacote CommonHelpers.InvokePrivate


##  1. Contexto e Motivação

Durante a manutenção e refatoração de bibliotecas legadas ou suítes de testes unitários onde o encapsulamento do código original não pode ser modificado de imediato, desenvolvedores recorrem frequentemente a Reflection crua (`Type.GetMethod(..., BindingFlags.NonPublic)`).

No entanto, a Reflection padrão em .NET embrulha qualquer exceção disparada dentro do método invocado em uma `TargetInvocationException`. Isso corrompe a legibilidade das falhas nos relatórios de teste e oculta a real exceção de negócio (`ArgumentException`, `InvalidOperationException`).


##  2. Decisões Arquiteturais

### 2.1. Desembrulho Automático com `ExceptionDispatchInfo`
- O `MethodInvoker` captura a `TargetInvocationException.InnerException` e utiliza `ExceptionDispatchInfo.Capture(inner).Throw()` para relançar a exceção original de negócio, **preservando integralmente a StackTrace original**.

### 2.2. Invocação Síncrona e Assíncrona (`async/await`)
- `MethodInvoker.InvokePrivateMethod<TReturn>(instance, methodName, params)`: Para métodos síncronos com retorno tipado ou void.
- `MethodInvoker.InvokePrivateMethodAsync(instance, methodName, params)`: Para métodos assíncronos que retornam `Task` ou `Task<T>`, aguardando a conclusão correta.

### 2.3. Diretriz de Governança e Anotação `[Obsolete]`
- O componente foi marcado com `[Obsolete]` com mensagem explicativa e avisos no `README.md`, orientando que seu uso é restrito a **testes de regressão e compatibilidade com legados**.
- Para novo código de produção, a boa prática orienta o uso do modificador `internal` combinado com `[InternalsVisibleTo]`.


##  3. Consequências e Trade-offs

###  Vantagens:
- Diagnósticos de teste limpos e sem poluição de `TargetInvocationException`.
- Totalmente compatível com .NET 6, .NET 8 e .NET 9 sem nenhuma biblioteca externa.


##  4. Status de Verificação
- Coberto por **12 testes unitários** no `CommonHelpers.InvokePrivate.Tests` (100% passing).

