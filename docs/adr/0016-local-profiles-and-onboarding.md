# 0016 — Yerel çocuk profilleri ve ilk kurulum akışı

- Durum: Kabul edildi
- Tarih: 2026-10-08
- İlgili: F1-10; UYM-01, VEL-01; PRD akış A1; CLAUDE.md altın kural 1, §11; ADR 0009, 0010, 0015

## Bağlam

Çocuk profili oluşturulmadan önce veli aydınlatma metnini görüp açık rıza vermeli (UYM-01). Ücretsiz oyun hesapsız sürer; bu yüzden rıza ve profiller önce cihazda tutulmalı, hesap açılınca sunucuya taşınmalı. Çocuk profilinde yalnız takma ad, avatar, yaş aralığı ve ilerleme olabilir.

## Karar

1. **Profiller cihazda tek dosyadadır:** `progress/profiles.json` (`ProfileRegistry`, saf C#). Alanlar yalnız takma ad (≤ 24 karakter), avatar kimliği, yaş aralığı (`minik`, `kasif`, `mucit`) ve bir profil kimliğidir. Her profilin ilerlemesi kendi dosyasındadır (`<kimlik>.json`, ADR 0009).
2. **Rıza cihazda kayıt altındadır:** hangi metin sürümüne, ne zaman. Metin sürümü değişince rıza yeniden istenir. Hesap açıldığında sunucuya `POST /v1/me/consents` ile iletilir (ADR 0015); o PR'a kadar yalnız yerelde tutulur.
3. **İlk açılış (A1):** Roboya sesle "bir büyüğün yardımına ihtiyacım var" der; ekranda yalnız büyük bir yetişkin düğmesi vardır. Düğme ebeveyn kilidini açar (ADR 0010) → aydınlatma metni → **açık rıza** → çocuk profili (takma ad, avatar, yaş aralığı) → ada. Metni reddetmek profil ve veri bırakmaz; karşılama ekranına döner. Rıza var ama profil yoksa metin atlanır.
4. **Profil yönetimi veli alanındadır (kilidin arkasında):** seçme, düzenleme, silme (iki dokunuş; ilerleme dosyası da silinir) ve ekleme. **Sınır:** ücretsizde 1, premiumda 4 (`ProgressRules`; sunucu da aynısını uygular, ADR 0013). İstemci premium hakkını kendisi vermez.
5. **Avatarlar kodla çizilir:** Roboya altı renkte; kimlik `robot-<renk>`. Renk tek başına anlam taşımaz, takma adla birlikte gösterilir.
6. **Eski kurulumlar:** profil dosyası yok ama önceki sürümün ilerleme kimliği varsa o ilerleme ilk profil olarak korunur (takma ad "Mucit"); rıza yine istenir.
7. **Bozuk dosya** yanına `.corrupt-<zaman>` olarak saklanır, oyun ilk kuruluma döner; çökmez.
8. **Metin tablosu:** yetişkin metinleri `content/localization/tr.json` (ADR 0010). Aydınlatma metni `content/legal/aydinlatma-metni.tr.md` dosyasından okunur ve uygulamayla birlikte paketlenir.

## Sonuçlar

- Aydınlatma metni **taslaktır**; gerçek kullanıcıya çıkmadan hukukçu onayı ve `status: final` gerekir. Yerel kopya da aynı dosyadandır, ayrı bir metin yoktur.
- Çocuk profili sunucuya yalnız hesap açılınca ve rıza kaydıyla gönderilir (ADR 0012, 0015); bu istemci akışı sonraki PR'larda.
- Yaş aralığı bölüm başlangıcını belirler (F1-08); bu PR yalnız saklar.
