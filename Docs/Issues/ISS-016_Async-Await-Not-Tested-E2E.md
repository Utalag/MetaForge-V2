# ISS-016 Async Method Body Await Not Tested E2E

Datum: 2026-07-09
PROP: PROP-043, PROP-045
Soubor: `Tests/MetaForge.Generators.Tests/CSharp/EndToEndScenariosTests.cs`
Závažnost: ⚠️ Střední
Stav: Open
Owner:
Poslední revize: 2026-07-09

## 1. Kontext

E2E scénáře 1-5 testují generátor, ale žádný scénář netestuje async metodu s tělem obsahujícím `await`. Scénář 4 (UserManagement) používá `IsAsync = true` pouze na **interface signaturách** — metody nemají tělo, takže `await` klíčové slovo není nikdy renderováno.

## 2. Popis problému

- `CodeGenerator.RenderMethodBody()` neví, že metoda je async — `_renderer.Render(method.Body)` volá ExpressionRenderer, který `AwaitExpression` umí vyrenderovat, ale nikdy nebyl otestován v kontextu async metody
- Kombinace `IsAsync = true` + `Body = BlockStatement(...)` + `AwaitExpression` nebyla nikdy otestována
- `RenderAwait` existuje v ExpressionRendereru, ale nevíme, zda negeneruje dvojité `await await` nebo jiné chyby

## 3. Dopad

- Pokud by async metody s awaitem negenerovaly správně, celá async pipeline (MCP, WebApi) by byla nefunkční
- Nízké riziko — renderer `AwaitExpression` je triviální, ale netestovaný

## 4. Doporučené řešení

Přidat E2E scénář 6: AsyncPipeline — třída s async metodou, která volá `await _repository.GetByIdAsync(id)` uvnitř `BlockStatement`. Pokryto v PROP-045.

## 5. Otevřené otázky

- Má `MethodElement.Body` vědět, že metoda je async, nebo to řeší jen signatura?

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*
