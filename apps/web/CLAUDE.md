# apps/web — Veli, öğretmen, kurum panelleri

Kök `CLAUDE.md` §9 geçerlidir.

- React + TypeScript (strict) + Vite + TanStack Query. npm workspace adı `@roboya/web`.
- API çağrıları yalnız `@roboya/api-contract` üretilmiş istemcisiyle.
- Özellik klasörleri: `src/features/{parent,teacher,admin}`, ortaklar `src/shared`.
- Metinler `src/i18n/tr.json`; tasarım belirteçleri `src/shared/tokens.css`.
- `npm run test`, `npm run lint`, `npm run typecheck`.
