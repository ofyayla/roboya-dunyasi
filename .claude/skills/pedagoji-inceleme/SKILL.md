---
name: pedagoji-inceleme
description: Bölüm, bölüm dizisi, ses metni veya PR için pedagoji ön incelemesi yap ve docs/pedagoji/incelemeler/ altına rubriğe dayalı rapor yaz. content/levels veya content/voice/script.csv değiştiğinde ya da pedagoji ekibine götürülecek bir konu hazırlanırken kullan.
argument-hint: <bölüm kimliği | oyun | dosya | PR | konu>
context: fork
agent: pedagoji-uzmani
---

Pedagoji ön incelemesi yap: $ARGUMENTS

1. Ajan tanımındaki "Her çağrıda önce oku" listesini oku (`docs/pedagoji/cerceve.md`, `rubrik.md`, `kararlar/`, PRD, CLAUDE.md §13).
2. Kapsamı belirle:
   - Bölüm kimliği (ör. `sabir-ormani.yon-avcisi.05`) → o bölüm ve patikadaki komşuları (önceki 3 ve sonraki 3 `order`).
   - Oyun veya bölge → o oyunun/bölgenin bütün bölümleri, `order` sırasıyla.
   - PR veya dal → aşağıdaki "Bu dalda değişen içerik dosyaları" listesindeki bölümler ve ses satırları ile onların patika komşuları.
   - Konu (ör. "Minik sırası") → ilgili bölümleri ve önceki incelemeleri (`docs/review/`, `docs/pedagoji/incelemeler/`) bul.
3. Bölüm JSON'larını ve ilgili `script.csv` satırlarını oku. Bölüm dizisi incelemesinde her yaş başlangıcından itibaren çocuğun yolculuğunu tablo olarak çıkar (sıra, oyun, en kısa çözüm, yeni şey, destek).
4. Rubriğin altı bakışını uygula; ajan tanımındaki rapor biçimiyle `docs/pedagoji/incelemeler/<bugün>-<kısa-konu>.md` dosyasını yaz.
5. Dönüşte: rapor yolu, kısa sonuç, `Engel` bulguları ve ekibe sorular.

## Bu dalda değişen içerik dosyaları

!`git diff --name-only main...HEAD -- content/levels content/voice/script.csv 2>/dev/null || true`
