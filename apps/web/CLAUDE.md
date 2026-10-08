# apps/web — Veli, öğretmen, kurum panelleri

Kök `CLAUDE.md` §9 geçerlidir.

- React 19 + TypeScript (strict, `noUncheckedIndexedAccess`) + Vite + TanStack Query. npm workspace adı `@roboya/web`.
- Metinler `react-i18next` ile `src/i18n/tr.json` dosyasından; bileşende sabit Türkçe metin yok.
- Yetişkin ekranlarında dokunma hedefi ≥ 48 px (`--touch-target`); renkler `src/shared/tokens.css` belirteçlerinden.
- API çağrıları yalnız `@roboya/api-contract` üretilmiş istemcisiyle.
- Özellik klasörleri: `src/features/{parent,teacher,admin}`, ortaklar `src/shared`.
- `npm run test`, `npm run lint`, `npm run typecheck`.
