# Roboya Dünyası: Minik Mucitler

3.5–10 yaş çocuklara hikâye içinde kodlama ve değerler öğreten, Türkçe seslendirmeli mobil oyun (iOS + Android) — [Roboya Kids](https://www.roboyakids.com.tr/).

- Ürün gereksinimleri: [docs/prd.md](docs/prd.md)
- Geliştirme planı: [docs/plan.md](docs/plan.md)
- Mimari kararlar: [docs/adr/](docs/adr/)
- Geliştirme kuralları: [CLAUDE.md](CLAUDE.md)

## Hızlı başlangıç

Gereksinimler: Docker, [uv](https://docs.astral.sh/uv/), Node 22+, .NET SDK 10, Git LFS, Unity Hub + Unity 6.

```bash
make setup      # bağımlılıklar, Docker servisleri, tohum verisi
make test       # tüm testler
make help       # tüm komutlar
```

## Repo haritası

| Klasör | İçerik |
| --- | --- |
| `apps/game` | Unity 6 oyun istemcisi |
| `apps/api` | FastAPI sunucusu |
| `apps/web` | React veli / öğretmen / kurum panelleri |
| `apps/level-editor` | İç kullanım bölüm editörü |
| `packages/level-schema` | Bölüm JSON şeması (tek kaynak) |
| `packages/api-contract` | OpenAPI ve üretilmiş istemciler |
| `content` | Bölümler, seslendirme senaryosu, yerelleştirme |
| `tools` | Doğrulayıcı, TTS, derleme betikleri |
| `infra` | Docker Compose, dağıtım |

## Güvenlik

Bu repo herkese açıktır. Sırlar (API anahtarları, mağaza kimlik bilgileri, imza dosyaları) **asla** commit edilmez; yalnız `.env.example` tutulur. Bir güvenlik açığı bulursanız lütfen herkese açık issue yerine bize e-posta ile ulaşın.
