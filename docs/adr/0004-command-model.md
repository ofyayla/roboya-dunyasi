# 0004 — Kodlama motoru komut modeli

- Durum: Kabul edildi
- Tarih: 2026-10-07
- İlgili: F0-06, F0-08, F0-09; OYN-03, OYN-04, YON-02, YZ-01; plan risk tablosu "Kodlama motorunun sonradan yetmemesi"

## Bağlam

Yedi oyunun hepsi aynı motoru kullanacak. Faz 0'da yalnız ileri/dön kartları gerekiyor, ama Döngü Dansı (tekrarla, dans hareketi), Robot Atölyesi (koşul, sensör), Mucit seviyesi (fonksiyon) ve Birlikte Başaralım (iki oyuncu) motoru çatallamadan desteklenmeli.

## Karar

- **Program bir komut ağacıdır:** `MoveCommand` (ileri, geri, sola, sağa), `RepeatCommand` (1–9 kez), `IfCommand` (`Condition` + then/else), `CallCommand` (`Program.Procedures` içindeki adlandırılmış gövde), `ActionCommand` (yerinde eylem; görünüm `ActionId`'ye göre canlandırır).
- **Koşullar** saf `Condition` sınıflarıdır (`PathBlockedAhead`, `OnItemColor`, `Not`). Yeni sensör = yeni alt sınıf.
- **Yorumlayıcı tembeldir:** `Interpreter.Run()` olayları tek tek üretir (`CommandStarted`, `Moved`, `Turned`, `Bumped`, `Collected`, `ActionPerformed`, `LoopIteration`, `Finished`). Görünüm her olayı canlandırıp bir sonrakini ister. Plan bütün olarak çalışır (YON-02).
- **Kart yolu (`CommandPath`)** iç içe kartı adresler; çarpma ve hata bu yolla bildirilir (OYN-03).
- **Başarı program bitince değerlendirilir:** hedefin üzerinden geçip devam etmek başarı değildir. Nesneler hücreye girilince otomatik toplanır.
- **Çarpmada robot yerinde kalır ve yürütme durur;** can veya süre kaybı yoktur (çocuk deneyimi kuralları).
- **Adım sınırı** (varsayılan 500) ve **çağrı derinliği sınırı** (16) sonsuz programları durdurur.
- **Çözücü** yalnız ilkel hareket kartları üzerinde genişlik öncelikli arama yapar; en kısa çözüm uzunluğu 3. yıldız ve doğrulama için kullanılır. `HintAdvisor` ilk yanlış kartı ve yerine gelecek kartı bulur (YZ-01'in 2. ve 3. kademesi).

## Sonuçlar

- Bal Peşinde'nin "bellek" davranışı (BAL-02) oyun katmanında çözülür: eski komutlar + yeni komutlar tek bir `Program` olarak çalıştırılır; `Run(from)` robotu kaldığı yerden yürütür.
- Döngü kartlı bölümlerde "en kısa kod" (tekrarla kartının sayılması) henüz hesaplanmıyor; Döngü Dansı (F3-01) öncesinde çözücüye eklenecek.
- Birlikte Başaralım için birden çok robot ve açılır/kapanır hücreler (köprü, kapı) gerekecek; bu, `RobotState` yerine çok ajanlı bir dünya durumu ve eşzamanlı adım yürüten bir zamanlayıcı ile eklenecek (F3-03 öncesi ayrı ADR).
- Nesne sayısı 64 ile sınırlıdır (toplanan nesneler 64 bitlik maskede).
