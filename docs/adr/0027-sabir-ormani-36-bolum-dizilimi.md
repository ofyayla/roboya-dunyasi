# 0027 — Sabır Ormanı: 36 bölümün taslak dizilimi

- Durum: Kabul edildi (taslak içerik; pedagoji onayı bekliyor)
- Tarih: 2026-10-09
- İlgili: F1-21; ADR 0018, 0025, 0026

## Karar

1. Bölge patikası **36 bölüm**: Yön Avcısı 12, Kodlama Kutusu 12, Bal Peşinde 12; tek yolda iç içe (`order` bölge içinde benzersiz). İlk üç bölüm Yön Avcısı'dır (ücretsiz katman, ADR 0018); sonrasında oyun her üç bölümde bir değişir, son bölüm Yön Avcısı finalidir.
2. **Zorluk, en kısa çözüm uzunluğundan türetilir:** ≤3 → zorluk 1 (Minik), 4 → 2 (Minik), 5–6 → 2 (Kaşif), 7–8 → 3, 9–10 → 4, ≥11 → 5. Doğrulayıcı "Minik ≤ 4 komut" kuralını denetler; bu yüzden 5+ kartlık bölümler Kaşif etiketlidir ve Kaşif başlangıcı 10. bölümdür.
3. Bölümler motor çözücüsüyle üretilip doğrulandı (BFS ile en kısa çözüm); her bölümün `alternativeLevelId`'si aynı oyunun bir önceki, daha zor olmayan bölümüdür.
4. Metinler `content/voice/script.csv` içindedir; ses dosyaları ElevenLabs ile üretildi (geçici, profesyonel kayıtla aynı anahtarlarla değişecek).
5. Pedagoji incelemesi için okunabilir liste: `docs/pedagoji/sabir-ormani-36-bolum-taslagi.md`.

## Sonuçlar

- **Taslak içeriktir.** Pedagoji ekibi sırayı, kavram dağılımını, hata avcısı sıklığını ve metinleri onaylayana kadar kapalı beta için kesin sayılmaz.
- Minik ve Kaşif için ayrı bölüm varyantları (PRD) yok; tek yol ve uyarlanabilir destek var.
- Yön Avcısı 10. bölümün "bölge finali" notu artık yanlış (24. sırada); bölge sonu ödülü 36. bölümde verilir.
- Tüm patika arayüzden çözülerek test edilir (`AllPrototypeLevels...`, birkaç dakika sürer).
