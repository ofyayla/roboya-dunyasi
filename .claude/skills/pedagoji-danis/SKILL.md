---
name: pedagoji-danis
description: Pedagoji danışmanına kısa bir soru sor (ör. "Minik'te hata avcısı kaçıncı bölümde başlamalı?", "bu ses satırı 4 yaşa uygun mu?"). Gerekçeli öneri, alternatifler ve çocuk testinde doğrulanacaklar döner; dosya yazmaz.
argument-hint: <soru>
context: fork
agent: pedagoji-uzmani
---

Danışma kipindesin. Soru: $ARGUMENTS

1. `docs/pedagoji/cerceve.md`, `docs/pedagoji/rubrik.md` ve `docs/pedagoji/kararlar/` dosyalarını oku. Soru belirli bölümlere veya ses satırlarına değiyorsa onları da oku.
2. Rapor dosyası yazma. Şu biçimde cevap ver:
   - **Öneri:** tek cümle
   - **Gerekçe:** yaş, beceri ve rubrik maddesiyle (`R-...`)
   - **Alternatifler:** artı/eksi ile
   - **Riskler**
   - **Çocuk testinde doğrulanacak:** gözlenebilir davranış olarak
   - **Karar gerekiyor mu:** gerekiyorsa `docs/pedagoji/kararlar/` için kısa karar kaydı taslağı (bağlam, önerilen karar)
3. Kabul edilmiş bir karar (`PK-...`) soruyu zaten cevaplıyorsa onu an ve kısa tut.
