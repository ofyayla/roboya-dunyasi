# Seslendirme hattı (F0-15)

Minik seviyede ekranda yazı yoktur; bütün yönergeler seslidir (OYN-01). Sesler anahtarla çağrılır, dosya adıyla çağrılmaz.

## Akış

1. Metin `content/voice/script.csv` dosyasına yazılır (`key,text,context,level_ids`). Bölüm JSON'u yalnız anahtarı taşır.
2. `make tts` eksik veya metni değişmiş satırları ElevenLabs ile üretir ve `content/voice/audio/tr/<key>.mp3` olarak kaydeder.
3. `content/voice/manifest.json` her satırın dosyasını, kaynağını (`tts` / `studio`), metin özetini (`textHash`), ses kimliğini ve modelini tutar.
4. `make validate-content` eksik, eskimiş (metni değişmiş) veya senaryoda olmayan sesleri hata olarak bildirir.
5. Oyun, manifest üzerinden anahtarı dosyaya çevirir. Editörde dosyalar doğrudan `content/voice`'tan okunur; cihaz derlemesinde StreamingAssets'e kopyalanır.

## Ayarlar

| Ayar | Değer |
| --- | --- |
| Sağlayıcı | ElevenLabs, model `eleven_multilingual_v2`, `language_code: tr` |
| Ses | "Jessica" (`cgSgspJ2msm6clMCkdW9`), 2026-10-08'de veliyle seçildi |
| Biçim | MP3 44.1 kHz 128 kbps (Git LFS) |
| Anahtar | Yalnız yerel `.env` (`ELEVENLABS_API_KEY`, `ELEVENLABS_VOICE_ID`); CI'da gerekmez |

## Gizlilik

Servise **yalnız senaryo metni** gider; çocuk veya kullanıcı verisi asla gönderilmez (CLAUDE.md §10). Kişiselleştirilmiş hikâyeler (F4-08) önceden üretilip insan onayından geçirilir; çalışma anında TTS çağrısı yapılmaz.

## Lisans uyarısı

ElevenLabs'in **ücretsiz planında üretilen sesler ticari kullanım için lisanslı değildir** ve atıf gerektirir. Prototip ve çocuk testleri için uygundur. **Kapalı betadan (F1-27) önce** ücretli bir plana geçilip bütün satırlar yeniden üretilmeli (ses kimliği aynı kalabilir; `make tts` metin değişmeden de ses/plan değişikliğinde yeniden üretmek için manifesti temizleyerek çalıştırılır). Lansmandan önce profesyonel kayıtlar aynı anahtarlarla `source: studio` olarak eklenir; kod değişmez.

## Stüdyo kaydına geçiş

Kayıt dosyası `audio/tr/<key>.mp3` olarak konur ve manifestte ilgili satırın `source` alanı `studio` yapılır. `make tts` stüdyo kayıtlarının üzerine asla yazmaz. Metin sonradan değişirse doğrulayıcı "yeniden kaydedin" uyarısı verir.
