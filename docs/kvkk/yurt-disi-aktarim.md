# KVKK m.9 — Kişisel verilerin yurt dışına aktarılması (özet)

> 2026-10-08 tarihli araştırma özeti; hukuki görüş değildir. Lansman öncesi hukukçu kontrolünde (F2-17) doğrulanacak. Birincil kaynaklar: Resmî Gazete ve [kvkk.gov.tr](https://www.kvkk.gov.tr).

## Çerçeve

- **7499 sayılı Kanun** (RG 12.03.2024; m.9 hükümleri 01.06.2024'te yürürlüğe girdi) KVKK m.9'u yeniden yazdı. Önceki rejimde aktarım için kural olarak açık rıza gerekiyordu; yeni rejimde açık rıza istisna, kurumsal güvenceler ana yol oldu.
- Aktarımdan önce, işlemenin kendisi için m.5 veya m.6'daki şartlardan biri bulunmalıdır. Aktarım mekanizması bunun yerine geçmez.
- Usul: **Kişisel Verilerin Yurt Dışına Aktarılmasına İlişkin Usul ve Esaslar Hakkında Yönetmelik** (RG 10.07.2024, sayı 32598).
- Kurum rehberi: **Kişisel Verilerin Yurt Dışına Aktarılması Rehberi** (Yayın No: 48, Ocak 2025).

## Üç kademe

1. **Yeterlilik kararı:** Kurul'un ülke, ülke içindeki sektör veya uluslararası kuruluş için verdiği karar. İkincil kaynaklara göre 2026 itibarıyla ilan edilmiş yeterlilik kararı yoktur. AB ülkeleri de bu kapsamda değildir.
2. **Uygun güvenceler** (yeterlilik kararı yoksa ve ilgili kişi Türkiye'de de haklarını kullanabiliyorsa):
   - **Standart sözleşme.** Kurul kararı 04.06.2024, 2024/959. Dört modül var: VS→VS, VS→Vİ, Vİ→Vİ, Vİ→VS. Bulut barındırma için uygun olan **Modül 2 (veri sorumlusundan veri işleyene)**.
     - Metin değiştirilemez; yalnız seçimlik hükümler belirlenir.
     - Tarafların yetkili temsilcilerince imzalanır; imza yetki belgeleri saklanır ve bildirimde sunulur.
     - İmzaların tamamlanmasından itibaren **5 iş günü içinde Kurum'a bildirilir** (m.9/5). Süre kanunda olduğu için idari düzenlemeyle esnetilemez.
     - Bildirimi sözleşmede belirlenen taraf yapar; belirlenmemişse veri aktaran yapar.
     - Bildirim yapılmazsa idari para cezası uygulanır. 2026 tutarları kaynaklarda yaklaşık **90.308 TL – 1.806.177 TL** olarak geçiyor; kaynaklar arasında küçük fark var, Resmî Gazete'deki yeniden değerleme ilanından teyit edilmeli.
   - Bağlayıcı şirket kuralları (Kurul onaylı) ve Kurul izinli yazılı taahhütname de birer güvencedir; bizim ölçeğimizde gerçekçi değiller.
3. **Arızi aktarım (m.9/6):** Yeterlilik kararı ve uygun güvence yoksa, **yalnız arızi (düzenli olmayan) aktarımda** kullanılabilir. Örnekler: olası riskler hakkında bilgilendirilmiş ilgili kişinin açık rızası, ilgili kişiyle yapılan sözleşmenin ifası. **Sürekli barındırma veya düzenli servis kullanımı için dayanak olamaz.**

## Bulut sağlayıcılar

- Sağlayıcının kendi DPA'sı veya "global privacy addendum" belgesi KVKK m.9 standart sözleşmesinin **yerine geçmez**; Kurul metniyle imzalanmış standart sözleşme gerekir.
- AWS'nin standart sözleşme imzaladığına dair yalnız ikincil ve gayriresmî bir kaynak bulundu. Azure için bir kullanıcının karşı imza talebi var, sonucu belirsiz. **Her sağlayıcıdan Modül 2'yi Kurul metniyle imzalayacağına dair yazılı teyit alınmalı.**
- Sağlayıcı veriyi yalnız bizim talimatımızla işliyorsa veri işleyendir (Modül 2). Kendi amaçları için de işliyorsa rol analizi değişir.

## Çocuk verisi

- KVKK'da çocuklara özgü bir aktarım hükmü bulunamadı. Topladığımız çocuk verisi (takma ad, avatar, yaş aralığı, ilerleme) **özel nitelikli kişisel veri değildir**.
- Çocuk adına haklar ve aydınlatma **veli** üzerinden yürür. Aydınlatma metni yurt dışı aktarımı veliye açıkça anlatmalıdır.
- **Okul modunda** okul veri sorumlusu, Roboya Kids veri işleyendir (UYM-02). Yurt dışındaki barındırma sağlayıcısı "alt veri işleyen" olur:
  - Okul veri işleme sözleşmesi bunu ve aktarım mekanizmasını belirtmeli.
  - Gerekiyorsa Vİ→Vİ (Modül 3) değerlendirmesi yapılmalı.
- Ek koruma (yasal zorunluluk değil, ürün kararı): en az veri ilkesi (CLAUDE.md altın kural 1), sunucuda şifreleme, takma ad ve anonim kimliklerle analitik.
- **Hukukçuya sorulacak:** okul profillerindeki çocuk verisinin alt veri işleyen eliyle AB'de tutulması için okulların ek onayı ve veli bilgilendirmesi gerekiyor mu; okul sözleşmesinde hangi modül kullanılmalı?

## Kaynaklar

- [KVKK — Kişisel Verilerin Yurt Dışına Aktarılması Rehberi (Yayın No: 48)](https://www.kvkk.gov.tr/Icerik/8142/Kisisel-Verilerin-Yurt-Disina-Aktarilmasi-Rehberi)
- [Güzeloğlu Hukuk — 7499 sayılı Kanun ile değişen KVKK m.9](https://www.guzeloglu.legal/tr/haber-makale/kisisel-verilerin-yurt-disina-aktarilmasi-7499-sayili-kanun-ile-degisen-kvkk-madde-9-ve-yeni-aktarim-rejimi-4485.html)
- [Halil Bakırcı — Standart sözleşme bildirimi: 5 iş günü](https://halilbakirci.av.tr/kvkk-standart-sozlesme-bildirimi-5-is-gunu-2026/)
- [Halil Bakırcı — Yeterlilik kararı](https://halilbakirci.av.tr/kvkk-yeterlilik-karari-yurt-disina-veri-aktarimi-madde-9-2026/)
- [Halil Bakırcı — Yabancı bulut ve SaaS](https://halilbakirci.av.tr/yabanci-bulut-saas-kvkk-yurt-disina-veri-aktarimi-2026/)
- [Güneş Partners — 7499 sonrası yeni sistem](https://www.gunespartners.com/makale/yurt-disina-veri-aktarimi-7499-sayili-kanun)
- [İmer Hukuk — Standart sözleşme rehberi](https://imer.av.tr/kvkk-araclari/yurt-disina-veri-aktarimi-standart-sozlesme-rehberi)
- [Birasyo — Yeni yönetmelik ve 5 iş günü kuralı](https://birasyo.com/tr/blog/kvkk-yurt-disi-veri-aktarimi-yeni-yonetmelik-standart-sozlesme/)
- [Mondaq — 2025 idari para cezaları ve rehber](https://www.mondaq.com/turkey/data-protection/1567704/2025-y%C4%B1l%C4%B1nda-uygulanacak-kvkk-%C4%B0dari-para-cezalar%C4%B1-ve-ki%C5%9Fisel-verilerin-yurtd%C4%B1%C5%9F%C4%B1na-aktar%C4%B1lmas%C4%B1na-%C4%B0li%C5%9Fkin-rehber-hakk%C4%B1nda-b%C3%BClten)
- [Erdem & Erdem — Yurt dışı aktarım rehberi neleri düzenliyor](https://www.erdem-erdem.av.tr/bilgi-bankasi/yurt-disina-kisisel-veri-aktarimi-rehberi-neleri-duzenliyor)
- [Microsoft Q&A — KVKK standart sözleşme onayı](https://learn.microsoft.com/tr-tr/answers/questions/5518137/kvkk-standart-s-zle-mesi-onay)
- [Netta — Sunucunuz yurt dışındaysa](https://nettacompany.com/blog/kvkk-yurt-disina-veri-aktarimi-sunucunuz-yurt-disindaysa)
