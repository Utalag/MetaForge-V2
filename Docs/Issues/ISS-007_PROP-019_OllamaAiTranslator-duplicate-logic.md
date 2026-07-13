# ISS-007 OllamaAiTranslator duplikuje logiku z OllamaAdapter

Datum: 2026-04-07
PROP: PROP-019
Soubor: `Src/MetaForge.Translator/Services/OllamaAiTranslator.cs`
Závažnost: ⚠️ Nízká
Stav: Accepted — By design (PROP-019 Variant A)
Owner:
Poslední revize: 2026-04-07

## 1. Kontext

Issue zjištěno při Code Review po implementaci PROP-019 (Translator — IAiTranslator a AI-assisted překlad).

## 2. Popis problému

`OllamaAiTranslator` duplikuje logiku z `MetaForge.Ai/Adapters/OllamaAdapter` — oba volají Ollama HTTP API. PROP-019 explicitně zvolil Variantu A (přímé volání), takže duplicita je vědomá, ale dlouhodobě neudržitelná.

## 3. Dopad

- Duplicita HTTP klient logiky — údržba dvou míst.
- Při změně Ollama API je nutné upravit obě místa.
- Zvyšuje riziko nekonzistence.

## 4. Doporučené řešení

Až bude MetaForge.Ai stabilní a jeho API se ustálí, sjednotit volání přes `OllamaAdapter` a `OllamaAiTranslator` převést na jeho wrapper.

## 5. Otevřené otázky

- Kdy bude MetaForge.Ai považováno za stabilní?

## 6. Rozhodnutí

*(Vyplní se po rozhodnutí usera/ownera.)*
- 2026-07-12: Duplicita je vědomá — PROP-019 explicitně zvolil Variantu A (přímé HTTP volání bez závislosti na MetaForge.Ai). Sjednocení proběhne až po stabilizaci MetaForge.Ai API.

---

## Související

- Vazby: `PROP-019`
