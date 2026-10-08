# 0020 — İstemci eşitlemesi: rıza, profiller ve yıldızlar

- Durum: Kabul edildi (insan incelemesi gerekli: çocuk verisi sunucuya gidiyor)
- Tarih: 2026-10-09
- İlgili: F1-15 (istemci), ILR-01; UYM-01, UYM-03; ADR 0012, 0015, 0016, 0019; CLAUDE.md altın kural 1

## Bağlam

Sunucu en yüksek yıldızı koruyan birleştirme uçlarını sağlıyor (ADR 0012). İstemci hesap açıldığında profilleri ve ilerlemeyi bu uçlarla eşitlemeli; veli bir profili silince sunucudaki kopya da gitmeli.

## Karar

1. **Ne zaman:** uygulama açılışında, bir bölümden haritaya dönüşte, girişten hemen sonra ve veli alanındaki "Şimdi eşitle" düğmesiyle. Hesap yoksa, çevrimdışıysa veya sunucu yapılandırılmamışsa hiçbir şey yapmaz; oyun etkilenmez.
2. **Rıza önce:** cihazda geçerli aydınlatma metnine rıza yoksa çocuğa dair hiçbir veri gönderilmez (`ConsentNeeded`). Rıza varsa `POST /v1/me/consents` (sunucuda idempotent) ile iletilir; sürüm uyuşmazlığı `NoticeOutdated` olur.
3. **Gönderilenler:** yalnız izinli çocuk alanları (takma ad, avatar, yaş aralığı; `PUT /v1/me/profiles/{id}`) ve bölüm kimliği → yıldız (`POST …/progress/sync`). Gelen birleşik yıldızlar yerel kayda yalnız yükselecek biçimde işlenir (`ProgressBook.Record`); yerel veri asla düşürülmez.
4. **Silme:** sunucuda bulunduğu bilinen profil kimlikleri `sync.json` dosyasında tutulur. Yerel kayıttan kalkan profil, sonraki eşitlemede `DELETE` ile sunucudan silinir; ağ hatasında kayıt kalır, tekrar denenir.
5. **Profil sınırı:** sunucu `profile_limit` (ücretsizde 1) derse o profil yalnız yerelde kalır, sonuç `PartlyDone` olur ve veliye cümleyle söylenir. İstemci premium hakkı vermez (ADR 0013).
6. **Çakışma:** sunucu başka hesaba ait kimlik için `not_found` döner; profil atlanır.

## Sonuçlar

- Veli, hesaptan çıksa bile yerel veriler cihazda kalır; çıkış sunucu verisini silmez (silme: gizlilik merkezi, sonraki PR).
- Yıldız dışı ilerleme (`shipPartsSeen`) cihaza özeldir, sunucuya gitmez; gemi parçaları yıldızlardan yeniden hesaplanır.
- Rıza geri çekme akışı gizlilik merkezi PR'ında istemciye eklenecek.
