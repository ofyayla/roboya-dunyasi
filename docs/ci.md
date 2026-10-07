# CI / CD

Tanım: [`.github/workflows/ci.yml`](../.github/workflows/ci.yml). Her PR'da ve `main`'e her birleşmede çalışır; kırmızıyken birleştirme yapılmaz.

| İş | Ne kontrol eder |
| --- | --- |
| Secret scan | gitleaks ile tüm geçmişte sır taraması |
| API | ruff, ruff format, mypy, Alembic göçleri, pytest (gerçek PostgreSQL 16) |
| Coding engine + validator | `make test-engine` (≥ %90 satır kapsamı) ve doğrulayıcı testleri |
| Content | şema testleri, `make validate-content`, `make check-gen` (üretilmiş kod güncel mi) |
| Unity EditMode | GameCI ile EditMode testleri (Unity projesi ve lisans sırrı varsa) |

## Unity lisansı (GitHub Secrets)

Repo herkese açık olsa da Secrets yalnız repo sahibine görünür ve fork'lardan gelen PR'lara verilmez.

1. Yerelde Unity Hub ile giriş yapılmış ve Personal lisans etkin olmalı.
2. Lisans dosyasını bulun: `/Library/Application Support/Unity/Unity_lic.ulf`.
3. GitHub → Settings → Secrets and variables → Actions:
   - `UNITY_LICENSE`: `.ulf` dosyasının tüm içeriği
   - `UNITY_EMAIL`: Unity hesabı e-postası
   - `UNITY_PASSWORD`: Unity hesabı parolası

Bu değerler hiçbir zaman repoya, sohbete veya loglara yazılmaz.

## Sonraki adımlar

- Ana dala birleşmede staging'e API ve web imajı dağıtımı (barındırma ADR'si sonrası, F0-19).
- Android iç test kanalı ve gece iOS/TestFlight derlemesi (Fastlane, Faz 1).
