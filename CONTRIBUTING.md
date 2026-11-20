# Contributing / Katkı rehberi

Thanks for helping! / Katkın için teşekkürler!

## Ways to help / Katkı yolları
- Report bugs and suggest features via [Issues](https://github.com/ozkurtomer/Nabiz/issues)
- Add a **language**: UI texts are in `web/src/app/core/i18n/` (Turkish text is the key), email and API messages in `api/Nabiz.Application`
- Add **question packs** in `api/Nabiz.Infrastructure/Seed/SoruSeed.cs`
- Improve docs, tests, accessibility

## Setup
```bash
docker compose up -d db mailpit          # database + fake SMTP
cd api && dotnet run --project Nabiz.Api  # API on :5277
cd web && npm install && npm start        # web on :4200
```

## Before opening a pull request / PR öncesi
```bash
cd api && dotnet test Nabiz.Tests
cd web && npx ng build
```
- Keep changes focused; add or update tests for behaviour changes.
- UI text is bilingual: every new Turkish string needs an English entry in `en.ts`.
- Database changes need an additive EF migration (`dotnet ef migrations add <Name> ...`).
- Design: flat, light, corporate. No dark theme, no decorative emoji in the UI.
- Commit style: conventional commits (`feat:`, `fix:`, `docs:`, `test:`, `ci:`).

## Privacy
Never commit real personal data, secrets or `.env.gercek`. Use the demo data generator for screenshots.

By contributing you agree that your work is released under the [MIT License](LICENSE).
