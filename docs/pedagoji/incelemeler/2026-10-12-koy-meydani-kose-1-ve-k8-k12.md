# Köy Meydanı (ADR 0031): Köşe 1 ve Köşe 2–4 tasarım kararları K8–K12, pedagoji ön incelemesi

> Bu bir yapay zekâ ön incelemesidir; pedagoji ekibinin onayının yerine geçmez.

- Tarih: 2026-10-12
- Kapsam: ADR 0031 (yönsüz Köy Meydanı); `content/levels/paylasim-koyu/koy-meydani/01–06.json`; `content/voice/script.csv` içindeki `koy_meydani.*`, `value.sharing` ve `count.*` satırları; motor `CodingEngine/Sharing/*`, ekran `Games/Common/Square/*`, `Sharing.uss`, doğrulayıcı `SharingChecks.cs`; ekran görüntüleri `apps/game/TestResults/screens/25–32-square-*.png` (33 numaralı görüntü bulunamadı); `docs/pedagoji/bekleyen-kararlar.md` K8–K12.
- İlgili: F2-13, BAL-01, BAL-03, OYN-01/03/04/05, YZ-01/02/03, KUT-02; PK-0001–PK-0005; bekleyen P1, P3, P4, P6.
- Önceki inceleme `2026-10-11-paylasim-koyu-kose-1-minik.md` ızgaralı köy için yazılmıştı. Artık tarihçe niteliğinde.

## Kısa sonuç

Yönsüz meydan, Minik için ızgaralı köyden belirgin biçimde daha uygun. Yön yükü kalkıyor, sayı tek biçimde (petek hücresi ve nokta) gösteriliyor, etkileşim tek dokunuşa iniyor ve dünya her çalıştırmada başa dönüyor. Köşe 1'de çocuğun ilerlemesini durduran bir **Engel bulunmadı.** En önemli üç bulgu şunlar:

1. **6. bölüm** 5 kart ve 5 bal istiyor. Bu, Minik aralığını (2–4) ve PK-0005'teki "3'e kadar sayma, 1 + 1" sınırını aşıyor. 4. bölümün sesi de "iki ve iki, dört" toplamasını adlandırıyor.
2. **Peteğe dokununca petekte bir şey olmuyor;** kart alttaki şeride düşüyor. Kart, petek ve hücre arasındaki bağı Minik çocuk yalnız simgeden kurmak zorunda. 6. bölümde karışık sıralı beş kartı simgeye göre ayırıp saymak gerekiyor.
3. **Başarısız her çalıştırmadan 1,8 sn sonra rehber el doğru adımı kendiliğinden gösteriyor.** Bu YZ-01'in kademelerini atlıyor. Minik için kabul edilebilir, ama Kaşif köşelerinde hata ayıklamayı ve K10'daki tahmini boşa çıkarır. Bu yardım ipucu sayılmadığı için YZ-02 de çocuğu olduğundan daha bağımsız görüyor.

K8–K12'nin yönü pedagojik olarak savunulabilir. Ancak her köşe hem yeni bir matematik fikri hem yeni bir kodlama mekaniği getiriyor; ikisi köşe içinde ayrı bölümlere dağıtılmazsa "bir bölümde tek yenilik" ilkesi bozulur. Ayrıca tekrar kutusu (K11), üründeki **ilk döngü** olacak: Sabır Ormanı'nda hiç döngü yok.

## Çocuğun yolculuğu: Köşe 1 (Minik, 1. bölümden)

Bütün bölümlerde amaç `deliver`; çiçek, sepet sınırı ve sayılı kart yok. Sepetteki bal, ihtiyaçların toplamına tam eşit.

| Sıra | Petekler (ihtiyaç) | Sepet | En kısa | `maxCards` | Yeni olan | Destek |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | ●, ● | 2 | 2 | 3 | Oyunun tamamı: peteğe dokun → kart, oynat, sepet, hücre, simgeler | `intro`, `guided`: el her adımı gösteriyor |
| 2 | ●, ●, ● | 3 | 3 | 4 | Üçüncü petek ve üçüncü simge | 8 sn beklemede el; alternatif: 1 |
| 3 | ●, ●● | 3 | 3 | 4 | İlk kez bir petek 2 bal istiyor (aynı peteğe iki dokunuş) | 8 sn el; alternatif: 2 |
| 4 | ●●, ●● | 4 | 4 | 5 | İki petek ikişer; toplam 4, ses "dört" diyor | 8 sn el; alternatif: 3 |
| 5 | ●●● | 3 | 3 | 4 | Tek petek 3 bal (3'e kadar sayma) | 8 sn el; alternatif: 3 |
| 6 | ●, ●●, ●● | 5 | **5** | 6 | Köşe finali; değer kartı ve gemi parçası | 8 sn el; alternatif: 4 |

Bir oturumda (Minik için 10–15 dk) köşenin tamamı oynanabilir. Yenilikler 1, 3 ve 5. bölümde geliyor; bu dağılım iyi. Zorluk eğrisinde 4'ten 5'e kısa bir düşüş, 6'da ise aralık dışına bir sıçrama var (Bulgu 1 ve 6).

## Bulgular

| # | Önem | Bakış | Bulgu | Dayanak | Öneri | İnsan kararı |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Önemli | Gelişimsel uygunluk | 6. bölüm 5 kart, 5 bal; 4. bölüm toplamı 4 ve sesi "iki ve iki, dört" | R-GEL-1, R-GEL-2, PK-0005 md. 1, ADR 0031 md. 3 | 6. bölüm: ●, ●, ●● (4 bal, 4 kart). 4. bölüm sesinden toplamı çıkar. Doğrulayıcıya "Minik en kısa ≤ 4" kuralı | Evet (PK-0005 md. 1'in yorumu) |
| 2 | Önemli | Gelişimsel uygunluk / Görsel dil | Peteğe dokununca petekte bir şey olmuyor; kart şeride gidiyor. Kart, petek ve hücre bağı yalnız simgeyle kuruluyor | R-GEL-2, R-GEL-4, çerçeve "kart → hareket hemen görülmeli" | A: kart petekten şeride uçsun. B: Minik'te planlanan bal hücrede soluk görünsün. C: şerit kartları peteğe göre kümelesin | Evet (B, P4 ile ilişkili) |
| 3 | Önemli | Görsel dil / Ses | Ekran görüntülerinde sepetteki bal okunmuyor; Ari ile sepet başlangıç yerinde plan şeridinin altında kalıyor. Ses ise "sepette iki bal var" diyor | R-DIL-3, R-ERS-3, R-GEL-4 | Ari'nin başlangıç yeri şeridin üstüne alınsın; nokta hapı tablette doğrulansın. Köşe 3'ten önce zorunlu | Hayır |
| 4 | Önemli | Hata ve motivasyon | Rehber el başarısız her çalıştırmadan 1,8 sn sonra ve 8 sn beklemede doğru adımı gösteriyor; bu yardım ipucu sayılmıyor | YZ-01, OYN-05, YZ-02, R-MOT-2, ADR 0031 md. 9 | Minik'te kalsın. Kaşif'te kademeli olsun: ses → yanlış kart → el. Otomatik el de ipucu sayılsın | Evet |
| 5 | Öneri | Kavram ilerleyişi | ADR'ye göre Köşe 1'in kodlama fikri "sıralama", oysa 6 bölümün hiçbirinde kart sırası sonucu değiştirmiyor | R-ILR-4, ADR 0031 md. 3 | Fikir dürüstçe adlandırılsın: "önce plan, sonra çalıştır; bir kart, bir bal" | Hayır |
| 6 | Öneri | Kavram ilerleyişi | 4 kartlık 4. bölümden sonra 3 kartlık 5. bölüm geliyor; final, 2+2'den hemen sonra üç peteğe çıkıyor | R-ILR-4 | 4 ile 5'in yeri değişsin (●●● önce, ●●/●● sonra) | Hayır |
| 7 | Öneri | Ses ve dil | Tutarsız ya da belirsiz satırlar: "peteği" tekil, "yeşil düğme" ile "oynat düğmesi" karışık, "Hangi arının peteğinde bal yok?" ayırt edici değil, "Bu petek" tekil, "ikisi iki bal" belirsiz | R-DIL-2, R-DIL-4, R-ERS-1 | Ayrıntılarda yeniden yazım önerileri | Hayır |
| 8 | Öneri | Gelişimsel uygunluk | İstek balonu ihtiyaç kaç olursa olsun **bir** damla gösteriyor; 3. bölümde "1 ile 2" ayrımına ters düşebilir | R-GEL-2, ADR 0031 Bağlam (sayının tek biçimi) | Balonda sayı gibi okunmayan bir simge (bal kavanozu) kullanılsın; çocuk testinde bakılsın | Hayır |
| 9 | Öneri | Değer | `value.sharing`: "paylaşmak, herkesin payını düşünmektir" tanım ve vaaz tonunda; "pay" Minik için soyut | R-DEG-3, R-DIL-1 | Somut gözlem diline çevrilsin | Hayır |
| 10 | Önemli | Kavram ilerleyişi (K8) | Her köşe bir matematik fikriyle bir kodlama mekaniğini birlikte getiriyor; Kaşif ilk kez Köşe 2'de çiçek, `al` kartı, sepet sınırı (ve belki sayılı kart) görüyor | R-ILR-1, R-ILR-3, R-ILR-5 | Köşe içinde yenilik takvimi (Ayrıntılar'daki tablo); doğrulayıcıya "tanıtım bölümünde tek yeni öğe" | Evet (K8) |
| 11 | Öneri | Gelişimsel uygunluk (K9) | Sepet sınırı (2) somut ve uygun. Riskler: yolculuklar arasında ara toplamı akılda tutmak ve sayılı kartla birleşmesi | R-GEL-2, R-ILR-1 | Sınır sabit 2 olsun, boş halkalarla görünsün; köşenin 3. bölümünde gelsin; sayılı kartla aynı köşede olmasın | Evet (K9) |
| 12 | Önemli | Hata ve motivasyon (K10) | Tahminin "deneme sayılmaz, yıldızı etkilemez" kuralı doğru ama tek başına yetmez | R-GEL-4, R-MOT-1, PK-0002, P3 | Yalnız ilk çalıştırmada sorulsun; tahmin bölümünde çiçek olmasın; tahmin sırasında el gösterilmesin; çocuk testine kadar bayrakla kapalı kalsın | Evet (K10, P3 ile birlikte) |
| 13 | Önemli | Kavram ilerleyişi (K11) | Tekrar kutusu üründeki ilk döngü. Köşe 4'te eşit paylaşma, "=" balonu, sofra ve döngü bir arada geliyor | R-ILR-1, R-ILR-3, R-ILR-5 | 19. bölüm rehberli, sofrasız, yalnız döngü olsun; sofra ancak 21–22'de gelsin; tur sayısı nokta ile gösterilsin; sayılı kart kapalı kalsın | Evet (K11) |
| 14 | Önemli | Ses ve dil / Motivasyon (K12) | Yalnız taşma olan başarısız bir çalıştırmada (Köşe 2–3) ses "Bu petek hâlâ bal bekliyor" diyor ama bekleyen petek yok. Taşmanın kendi sesi yok | R-DIL-3, R-MOT-1, PK-0001 md. 3 (ses dili) | Nötr bir taşma satırı eklensin; bekleme hâli doğru ama sinyal zayıf olabilir, çocuk testinde bakılsın | Evet (K12) |
| 15 | Öneri | Değer / Görsel dil | 25 numaralı hikâye görüntüsünde Roboya ile köy arısı ekran kenarlarında kesilmiş | R-DEG-2 | Son karede ihtiyacı anlatan karakter tam görünüyor mu, bakılsın (animasyon anında çekilmiş olabilir) | Hayır |

## Ayrıntılar

### Bulgu 1: 6. bölüm ve sayı sınırı

- **Gözlenen:** `06.json` içinde petekler `need` 1, 2, 2, sepet 5, `shortestLength` 5, `maxCards` 6. `04.json` içinde sepet 4; başarı sesi `koy_meydani.l04.success` "İki ve iki, dört bal!" diyor. `l06.intro` ise "Sepette beş bal var" diyor.
- **Neden önemli:** Çerçeveye göre Minik planı 2–4 kartı geçmemeli. Kabul edilmiş PK-0005 madde 1, Köşe 1'i "birebir eşleme, 3'e kadar sayma ve 1 + 1" ile sınırlıyor. K8 yalnız ızgara maddelerini geçersiz kıldı, bu madde ızgara maddesi değil. ADR 0031 madde 3 de "3'e kadar sayma" diyor. Teslim (`deliver`) hedefinde çocuğun toplamı sayması gerekmiyor; peteklerin her biri en çok 2–3 istiyor. Ama ses toplamı adlandırınca (4, 5) çocuğu o toplamı saymaya çağırıyor. 4–5 yaşta beşe kadar güvenilir sayma çocuktan çocuğa çok değişir, ve bu yük köşenin en uzun planıyla aynı bölüme düşüyor.
- **Öneri:**
  - 6. bölüm: ●, ●, ●● (sepet 4, 4 kart). Final yine üç peteği ve iki farklı ihtiyacı birleştirir; aralığın içinde kalır.
  - 4. bölüm başarı sesi: "İki petek de doldu! Köy arıları çok mutlu." Giriş sesindeki "Sepette dört bal var" cümlesi de çıkarılabilir. Bu sayının ekranda okunabilir olması Bulgu 3'e bağlı.
  - `SharingChecks.CheckMinik` şu an yalnız `maxCards > 6` için uyarı veriyor. Buna "en kısa ≤ 4" kuralı (hata) ve "toplam bal ≤ 4" kuralı (uyarı) eklensin.
- **İnsan kararı:** PK-0005 madde 1'deki "3'e kadar" peteğe göre mi, sepetin toplamına göre mi okunacak? Toplama göre okunursa 4. bölüm de değişmeli.

### Bulgu 2: Dokunuşun etkisi petekte görünmüyor

- **Gözlenen:** `SquareController.AddCard`, peteğe dokunulunca şeride bir kart ekleyip bir ses çalıyor; petekte görünür bir değişiklik olmuyor (26 ve 27 numaralı görüntüler). Kart ile petek arasındaki bağı yalnız simge kuruyor (kırmızı daire, mavi üçgen; renk ve biçim birlikte, R-ERS-1 sağlanıyor).
- **Neden önemli:** Bu yaşta neden-sonuç anlık ve aynı yerde görülmeli. Minik çocuk peteğe dokunup balın gitmesini bekleyebilir, gitmeyince yeniden dokunur ve şerit dolup sarsılır. 6. bölümde şerit örneğin ▲ ● ■ ▲ ■ olabilir. Çocuk "▲ peteği iki istiyordu, şeritte kaç ▲ var?" diye sormak zorunda: dağınık kartları önce simgeye göre ayırmak, sonra saymak. Bu iki işlemlik bir yük ve Minik'te asıl öğretilen birebir eşlemeyi gölgeliyor.
- **Seçenekler:**
  - **A. Kart petekten şeride uçsun** (simge ve damla birlikte). Ucuz bir çözüm, dokunuşla kartın kaynağını bağlar. Eksisi: hücreyle bağ kurmaz.
  - **B. Minik'te planlanan bal hücrede soluk görünsün.** Çocuk eksik hücreyi planlarken görür, birebir eşlemenin kendisi ekrana gelir. Eksisi: çalıştırmanın "sürprizini" azaltır ve Sabır Ormanı'ndaki yol gölgesi tartışmasının (P4) köydeki karşılığıdır. Yalnız Minik'te açık olabilir.
  - **C. Şerit kartları peteğe göre kümelesin** (aynı simgeler yan yana). Saymayı kolaylaştırır. Eksisi: sıranın hiç önemli olmadığını pekiştirir; Köşe 2'de sıra önemli olunca kümeleme kapatılmak zorunda kalır.
  - Ön tercihim, Minik için A ile B'nin birlikte kullanılması; Kaşif'te yalnız A. Bu, çocuk testinde doğrulanmalı.

### Bulgu 3: Sepetteki bal görünmüyor

- **Gözlenen:** `Sharing.uss` dosyasında `.basket__dots` sepetin soluna konan bir nokta hapı olarak tanımlı (`right: 100%`, `top: 36px`). 26, 27 ve 30 numaralı görüntülerde Ari ile sepet başlangıç yerinde (`Home()`: sahnenin altı) plan şeridinin altında kalıyor; hap görünmüyor, yalnız sepet ağzında iki küçük damla seçiliyor. Uçuş sırasında (28) da hap görünmüyor. Ekran görüntüleri çalışma ağacından eski olabilir, bu yüzden tablette doğrulanmalı.
- **Neden önemli:** `l01.intro` "Ari'nin sepetinde iki bal var" diyor; seste adı geçen şey ekranda okunmalı (R-DIL-3). Köşe 1'de çözüm için sepet sayısı gerekmiyor, bu yüzden önem derecesi "Önemli". Köşe 3'te ise tahmin ("sepette kaç kalacak?") tamamen bu sayıya dayanıyor; orada görünmezse **Engel** olur.
- **Öneri:** Ari'nin başlangıç yeri şeridin üstünde olsun ve sepet, noktalarıyla birlikte tam görünsün. Hap kapasite kadar boş halka gösterdiği için K9'daki sınırı da doğal olarak anlatır; bu korunsun.

### Bulgu 4: Rehber elin zamanlaması

- **Gözlenen:** `ExplainAsync`, başarısız her çalıştırmadan sonra `AfterRunHintMs = 1800` ile eli kendiliğinden açıyor; beklemede `IdleHintMs = 8000` var. El `SharingHint.Next` ile bir sonraki doğru adımı ya da çıkarılacak kartı gösteriyor. Düğmeyle istenen ipucu `HintsUsed` sayacını artırıyor, otomatik el artırmıyor.
- **Neden önemli:**
  - YZ-01 ve OYN-05, üç başarısızlıktan sonra üç kademeli ipucu istiyor (ses → hatalı kart → doğru kart). Otomatik el, ilk başarısızlıkta doğrudan 2. ya da 3. kademeye geçiyor.
  - Minik'te bu kabul edilebilir: Köşe 1 çok kısa ve takılmak öğretici değil.
  - Kaşif'te hata ayıklama ve planlanan ile gerçekleşeni karşılaştırma çekirdek beceri. El 1,8 sn içinde cevabı gösterirse çocuk düşünmeden eli izlemeyi öğrenir; K10'daki tahmin de anlamsızlaşır.
  - Otomatik el ipucu sayılmadığı için YZ-02, üç yıldızlı bölümleri "yardımsız" sayar ve desteği gereğinden erken azaltabilir. Üç yıldızın ölçütü de (OYN-04) anlamını yitirir.
- **Öneri:**
  - Minik (Köşe 1): bugünkü davranış kalsın.
  - Kaşif (Köşe 2–4): 1. başarısızlıkta yalnız bekleyen petek ve ses; 2. başarısızlıkta hatalı kart vurgulansın (el yok); 3. başarısızlıkta el. Bekleme süresi 15–20 sn olsun.
  - Otomatik el de `HintsUsed` sayacına (ya da ayrı bir "yardım aldı" bayrağına) yazılsın.
  - Bu, ADR 0031 madde 9'u değiştirir; ürün sahibiyle karar verilmeli.

### Bulgu 5: Köşe 1'in kodlama fikri

Altı bölümün hepsinde sepet ihtiyaçların toplamına eşit, çiçek ve sınır yok. Bu yüzden doğru kartların her sıralaması kazanıyor (ipucu motoru da "her en kısa yol geçerli" diye çalışıyor). Öğretilen kodlama fikri sıralama değil: **önce plan, sonra çalıştır; her kart bir eylem**. Bu, Minik için doğru ve Sabır Ormanı'ndaki "plan bütün olarak çalışır" fikrinin devamı. ADR 0031 madde 3 ve taslak bu adla güncellenirse, veli ve eğitmen beklentisi ile Köşe 2'nin "artık sıra önemli" geçişi daha net olur.

### Bulgu 6: 4. ve 5. bölümün sırası

5. bölüm (●●●, 3 kart), 4. bölümden (●●/●●, 4 kart) kolay. Önerilen sıra: ●/● → ●/●/● → ●/●● → ●●● → ●●/●● → final. Böylece "aynı peteğe birden çok bal" önce tek petekte, sonra iki petekte çalışılır. Mevcut sıra bir pekiştirme molası olarak da savunulabilir. Değişiklik isteğe bağlı.

### Bulgu 7: Ses satırları (öneri metinleri)

| Anahtar | Şimdi | Öneri | Neden |
| --- | --- | --- | --- |
| `l01.intro` | "Köy arılarının peteği bomboş! … Peteğe dokun, sonra yeşil düğmeye bas!" | "Köy arılarının petekleri bomboş! Ari'nin sepetinde iki bal var. Her peteğe bir bal verelim." (Eylem yönergesi rehber eldeyken ayrı bir satırda verilsin: "Peteğe dokun.") | İki petek var; bir yönergede tek iş |
| `hint.play` | "…yeşil düğmeye bas…" | "Hazırsan oynat düğmesine bas, Ari balı götürsün!" | `roboya.play_prompt` ile aynı ad; renk tek başına anlam taşımamalı |
| `hint.look` | "Hangi arının peteğinde bal yok?" | "Bak, her arının peteğinde boş yuvalar var. Her boş yuvaya bir bal." | Planlarken bütün petekler boş, soru ayırt etmiyor |
| `waiting` | "Bu petek hâlâ bal bekliyor." | Tek ve çoğul iki satır: "Bu arı hâlâ bal bekliyor." / "Bazı arılar hâlâ bal bekliyor." | Birden fazla petek bekleyebilir |
| `l06.intro` | "Biri bir, ikisi iki bal istiyor." | (Bulgu 1'deki yeni kurguya göre) "Bu arılar birer bal istiyor, bu arı iki bal istiyor." Söylenirken el ya da parıltı ilgili peteği göstersin | "İkisi iki" toplam gibi duyulabilir |
| `l02.success` | "Bir, iki, üç! Üç petek de doldu." | "Üç petek de doldu!" | Çalıştırma sırasında `count.*` her peteğin kendi balını sayıyor ("Bir!" "Bir!" "Bir!"); başarıda peteklerin sayılması iki sayma biçimini karıştırıyor |

### Bulgu 8: İstek balonu

`HiveView`, teslim hedefinde balona `need` kaç olursa olsun tek bir damla koyuyor. Sayı ise hücrelerde. Minik çocuk balondaki tek damlayı "bir bal istiyor" diye okuyabilir; 3. bölümde 2 isteyen peteğe tek kart koyması beklenebilir. Hücre sayısını balonda tekrarlamak ADR'nin "sayı tek biçimde" ilkesine aykırı olur. Bu yüzden balonda sayı gibi okunmayan bir simge (bal kavanozu) kullanmak daha güvenli. 30 numaralı görüntüde dolu kırmızı peteğin balonu da açık görünüyor; bu, `ShowGoal` sırasındaki 3,6 sn'lik pencereden kaynaklanıyor olabilir. Balonun dolu bir petekte görünmemesi doğrulansın.

### Bulgu 9: Değer anı

`value.sharing`: "Köyün yaşlı arısı der ki: paylaşmak, herkesin payını düşünmektir. Bugün herkese yetti!" İlk cümle bir tanım ve çocuğa ne olması gerektiğini söylüyor; "pay" sözcüğü 4 yaşta soyut. Değer mekaniğin içinde zaten var (Ari bütün sepetini köye veriyor). Öneri: "Ari bütün balını paylaştı, şimdi herkesin peteğinde bal var. Birlikte daha tatlı!" Ses, sahnede görülen şeyi anlatsın. Aynı satır her köşenin sonunda çaldığı için Köşe 4'teki sofra ile de uyumlu olmalı.

### Bulgu 10 (K8): Dört köşe, dört kavram

**Doğru bulduğum yanlar:**
- Yön işi Sabır Ormanı'nda kalıyor.
- Köy, matematiği tek biçimde (nokta ve hücre) gösteriyor.
- Kodlama fikirleri giderek ilerliyor: önce plan → sıranın önemli olduğu plan → çalıştırmadan önce izini sürme (tahmin) → döngü. Bu, çerçevedeki kavram ilerleyişiyle (sıralama → hata ayıklamaya hazırlık → döngü) uyumlu.

**Risk:** Her köşenin başında bir matematik fikri (toplama, çıkarma, eşit bölme) ile bir kodlama mekaniği (sınır, tahmin, tekrar kutusu) birlikte geliyor. Kaşif çocuk ayrıca köye Köşe 2'den giriyor (PK-0005 madde 2 ızgara maddesi değil, hâlâ geçerli varsayıyorum). Orada aynı anda çiçek, `al` kartı, sepet sınırı ve belki sayılı `ver ×n` kartı görüyor. Önerilen yenilik takvimi:

| Bölüm | Köşe 2 | Köşe 3 | Köşe 4 |
| --- | --- | --- | --- |
| 1. (tanıtım, rehberli) | Çiçek ve `al` kartı; sınır yok | Çıkarma: sepette bal artar, tahmin yok | Tekrar kutusu; 2 petek, kalan yok, sofra yok |
| 2. | İçinde bal olan petek: eksiği tamamlama | Aynı mekanikle pekiştirme | Daha çok tur |
| 3. | Sepet sınırı (rehberli an) | Tahmin adımı (rehberli an) | 3 petek |
| 4.–5. | Pekiştirme, farklı bağlam | Pekiştirme | Sofra ve artan (rehberli an), sonra pekiştirme |
| 6. (final) | Birleştirme | Birleştirme | Birleştirme, kutlama |

Sayılı kart (`amounts`) hangi köşede, hatta hiç kullanılacak mı, belirsiz. Kullanılacaksa ayrı bir tanıtım bölümü olmalı ve sınırla aynı bölüme düşmemeli. Eksik olan bir kavram da var: Kaşif'in çekirdeği olan hata ayıklama köyde hiç yok. Her Kaşif köşesine bir "Ari'nin planı karışmış" bölümü (hazır, tek yanlışlı plan) eklenebilir; bu bir öneri.

### Bulgu 11 (K9): Sepet sınırı

- **Uygun bulduğum yanlar:** Sınır fiziksel ve görünür (hapta boş halkalar), soyut değil. "Sepet dolu! Önce balı peteğe verelim" geri bildirimi o kartta durup kartı işaretliyor; bu, Kaşif için iyi bir hata ayıklama anı. `CheckBasketCap` sınırın planı gerçekten değiştirmesini zorunlu tutuyor; bu da doğru.
- **Riskler:**
  - (a) Sınır ancak bir peteğin ihtiyacı sınırı aşınca ya da birden çok çiçek olunca sırayı önemli kılıyor. O zaman çocuk bir peteğe yolculuklar arasında verdiğini akılda tutmak zorunda kalıyor (2 + 1). Canlı dolan hücreler bu yükü taşıyor; yine de ilk bölümlerde tek petek ihtiyacı sınırı aşsın.
  - (b) `al` kartı alabildiği kadar alıyor; çiçek 3 balla, sınır 2 iken çiçekte 1 kalıyor. Bu doğru ama örtük. İlk sınır bölümünde çiçekler sınırdan küçük olsun.
  - (c) Sınır ile sayılı kart birleşirse ("×2 ver" ile "2 al" aynı plan içinde) yük katlanır.
- **Öneri:** Köşe boyunca sınır sabit 2 olsun, ilk kez köşenin 3. bölümünde gelsin, sayılı kartla aynı köşede olmasın. Kaşif için "fazla soyut" değil; çocuk testinde gözlenmeli.

### Bulgu 12 (K10): Tahmin adımı

**Doğru bulduğum yanlar:** Tahmin, çocuğun **kendi planı** üzerinden hesaplanıyor (`SharingPrediction.LeftAfter(level, program)`). Böylece aritmetik hata ile plan hatası ayrışıyor. Çeldiriciler ("verdiğim", "başta vardı") tam da bu yaşın tipik yanılgıları; tanılayıcı değerleri var. Doğrulayıcı üçünün farklı olmasını zorunlu tutuyor. "Deneme sayılmasın, yıldızı etkilemesin" kuralı doğru (R-MOT-1). Bu adım, PK-0002'nin gerekçesindeki "zihinden çıkarma için soru çalıştırmadan önce sorulmalı" koşulunu da karşılıyor.

**Tek yargı yetmiyor.** Ek kurallar öneriyorum:
1. Bölüm başına yalnız ilk çalıştırmadan önce sorulsun (ya da ilk doğru tahmine kadar). Her çalıştırmada sormak oyunu yavaşlatır, tahmini angaryaya çevirir.
2. Tahmin bölümlerinde çiçek olmasın. Şu an `before = TotalHoney`, yani sepetteki bal ile çiçeklerdeki bal toplanıyor. Çiçek varsa "başta vardı" sayısı ekranda tek yerde durmuyor ve soru toplama artı çıkarmaya dönüşüyor.
3. Tahmin anında rehber el ve "oynat düğmesine bas" nabzı çalışmasın (Bulgu 4).
4. Sepetin sayısı ekranda okunmalı (Bulgu 3); bu sağlanmadan adım açılmasın.
5. Yanlış tahminden sonra ceza sesi olmasın. Seçilen kart ile sepet yan yana dursun; Ari ile birlikte sayılsın (PK-0002 madde 6 korunur).
6. Seçenekler önce nokta olsun, rakam ikincil kalsın (PK-0002 madde 5).
7. PK-0002 madde 3 ve P3'teki ön tercihle tutarlı olsun: adım çocuk testine kadar yapılandırmadan gelen bir bayrakla kapalı kalsın. Sabır Ormanı'nın P3 kararı ile aynı görüşmede ele alınsın; iki oyunda "tahmin" farklı kurallarla çalışırsa çocuk için tutarsız olur.

### Bulgu 13 (K11): Tekrar kutusu

- **Uygun bulduğum yanlar:**
  - Kutunun yuva sayısı petek sayısına eşit. Bu, "bir tur = herkese bir" stratejisini, yani bu yaşta eşit paylaşmanın doğal yolunu (PK-0001 madde 2'deki "bir ona, bir ona") doğrudan mekaniğe koyuyor. Güçlü ve yaşa uygun bir iskele.
  - Tur sayısı paylaşılan miktara denk geliyor. Çocuk bölme işlemini bilmeden deneme yoluyla bulabiliyor: tur az gelirse "Sepette bal kaldı. Onu da paylaşalım.", fazla gelirse sepet boşalıp plan duruyor. İki geri bildirim de yön gösteriyor.
  - Sofra kartının yalnız kutudan sonra durabilmesi, tekrarlanan ile tekrarlanmayan arasında ilk karşıtlığı veriyor.
  - `CheckRepeatBox` kutunun gerçekten gerekli olmasını zorunlu tutuyor.
- **Riskler:**
  - (a) `content/levels` içinde hiç `repeat` yok: bu, çocuğun gördüğü **ilk döngü** olacak. Döngü bir tanıtım bölümü ister (R-ILR-5) ve başka yeniliklerle birleşmemeli.
  - (b) Köşe 4'te `share` hedefi, "=" balonu, sofra ve tekrar kutusu yeni. Takvim Bulgu 10'daki tabloda.
  - (c) Çocuk kutuya ne koyacağına karar vermiyor; petek kartları otomatik olarak kutuya giriyor. Bu ilk döngü için uygun bir sadeleştirme, ama "döngüyü öğrendi" iddiası "tur sayısını ayarladı" düzeyinde kalır. Veli raporundaki dil buna göre ölçülü olmalı.
  - (d) `−/+` ile tur sayısı rakamla değil noktayla ya da tur simgesiyle gösterilmeli, sesle söylenmeli.
  - (e) `amounts` açıksa `[A×2, B×2]` ile kutusuz bir çözüm kutuyu gereksiz kılabilir. Doğrulayıcı bunu yakalar, ama tasarımda baştan kapalı olmalı.
  - (f) Yerleşim: 4 petek, 3 eşittir işareti ve sofra yan yana yaklaşık 1380 px tutuyor (`.hv` 212 px + 36 px kenar boşluğu, `.tb` yaklaşık 266 px), oysa referans genişlik 1280 px. Köşe 4'te 4 petek ve sofra birlikte kullanılacaksa ekranda doğrulanmalı.
- **5–7 yaş için anlaşılır mı?** Kutunun petek sayısı kadar yuvası olduğu ve turun "herkese bir" diye seslendirildiği sürece, yaşa uygun bir ilk döngü olarak savunulabilir. Asıl gözlenecek şey, çocuğun turu bir **sayı** olarak mı, "bir tur daha" diye **deneme** olarak mı kullandığı.

### Bulgu 14 (K12): Taşan bal ve bekleyen arı

- **Gözlenen:** `SetWaiting` beklemeyi bir halka, hafif bir zıplama ve istek balonuyla gösteriyor; köylü arı "Child" pozunda (neşeli) kalıyor (30 numaralı görüntü). Taşan bal peteğin altına damla olarak çiziliyor; `Spilled` olayı için ses ya da animasyon yok. `ExplainAsync` teslim hedefinde, çalıştırma durmadıysa her zaman `koy_meydani.waiting` satırını çalıyor.
- **Ses ile ekran uyuşmuyor:** Köşe 1'de sepet ihtiyaçlara eşit olduğu için taşma her zaman başka bir peteği eksik bırakıyor, sorun yok. Köşe 2–3'te ise sepette bal kalabildiği için çocuk bir peteğe fazla verip ötekileri tam doldurabilir. Bu durumda hiçbir petek beklemiyor ama ses "Bu petek hâlâ bal bekliyor" diyor ve asıl sorun (fazla verdin) adlandırılmıyor. Bu, Köşe 2–3 yazılmadan düzelmeli.
- **Öneri:** Bir `koy_meydani.spilled` satırı eklensin: "Bu petek doldu, fazlası yere aktı." Ton nötr olsun, "israf" ya da "kimse fazla almasın" dili kullanılmasın (PK-0001 madde 3'teki ses ilkesi hâlâ geçerli). Taşma ile bekleme birlikteyse önce taşma söylensin.
- **Bekleme hâli:** "Üzgün değil, bekliyor" yaklaşımı suçluluk yaratmamak için doğru. Ama neşeli pozdaki arı Minik'e "her şey yolunda" da diyebilir. Ayrı bir "bekliyor" pozu (ör. elinde boş kâse, sakin bakış) sinyali güçlendirir. Çocuk testinde bakılsın.
- **Ari'nin sofrada oturması:** Sofrayı "atılmış bal" değil ortak yer olarak gösteriyor; PK-0001 madde 3'ün niyetiyle uyumlu. Köşe 4 sonundaki sahnede köylü arıların da sofrada görünmesi önerilir.
- **Taşmada çalıştırmanın sürmesi** (ADR madde 4): Kaşif için doğru; çocuk sonucun tamamını görür. Minik için de sorun yok, çünkü dünya her çalıştırmada başa dönüyor.

## Ekibe sorular

1. **PK-0005 madde 1** (Köşe 1: "3'e kadar sayma ve 1 + 1") K8'den sonra geçerli mi? Sınır peteğe göre mi, sepetin toplamına göre mi? (Bulgu 1)
2. Minik'te dokunuşun petekte görünmesi için A, B ya da C seçeneklerinden hangisi denensin? B, Sabır Ormanı'ndaki P4 (yol gölgesi) ile birlikte mi ele alınsın? (Bulgu 2)
3. Rehber elin her başarısızlıktan sonra kendiliğinden gelmesi (ADR 0031 madde 9) Kaşif köşelerinde YZ-01'in kademeleriyle değiştirilsin mi? Otomatik el, YZ-02 için ipucu sayılsın mı? (Bulgu 4)
4. **K8:** Köşe içi yenilik takvimi (Bulgu 10'daki tablo) kabul mü? Sayılı `ver ×n` kartı hangi köşede kullanılacak, ya da hiç kullanılacak mı?
5. **K9:** Sepet sınırı sabit 2 mi olsun, köşenin 3. bölümünde mi gelsin?
6. **K10:** Tahmin bölüm başına bir kez mi sorulsun? Sabır Ormanı'nın P3 kararıyla birlikte mi verilsin? Çocuk testine kadar bayrakla kapalı kalsın mı?
7. **K11:** Ürünün ilk döngüsünün köyün son köşesinde gelmesi kabul mü? Kaşif döngüyü başka bir oyunda daha önce görmeli mi?
8. **K12:** Taşma satırının metni nasıl olsun? Bekleme için ayrı bir poz çizilsin mi?
9. Kaşif profil hâlâ önce 1. bölümü tanıtım olarak oynayıp Köşe 2'den mi başlıyor (PK-0005 madde 2)? Köşe 1'in Minik'e erken açılması (P1, P6) hâlâ açık.
10. `value.sharing` her köşe sonunda aynı mı kalacak, yoksa köşeye göre değişecek mi?

## Çocuk testinde gözlenecekler

- **Minik, 1–6. bölüm:**
  - Peteğe dokununca petekte bal bekliyor mu, aynı peteğe arka arkaya dokunuyor mu?
  - Şeritteki kartı peteğe parmakla eşliyor mu (simgeyi gösteriyor mu)?
  - 6. bölümde şeritteki aynı simgeli kartları sayarken yanılıyor mu?
- **İstek balonu:** 3. bölümde 2 isteyen peteğe tek kart koyup oynatıyor mu? Balondaki damlayı sayı gibi mi okuyor?
- **Sepet:** "Sepette iki bal var" duyunca sepete bakıyor mu, noktaları görebiliyor mu?
- **Rehber el:** Başarısız çalıştırmadan sonra eli mi izliyor, yoksa kendi hatasını mı arıyor? (Minik ile Kaşif ayrı not edilsin.)
- **Bekleme ve taşma:**
  - Bekleyen peteği fark ediyor mu ("hâlâ istiyor")?
  - Neşeli pozdaki arıyı "tamam" diye mi okuyor?
  - Yere akan bal için "döküldü, kötü oldu" gibi bir şey söylüyor mu?
- **Köşe 2 (K9):**
  - "Sepet dolu" anından sonra kartları yeniden mi diziyor, eksik `ver` kartını sona mı ekliyor?
  - Yolculuklar arasında peteğe verdiğini akılda tutabiliyor mu (2 + 1)?
- **Köşe 3 (K10):**
  - Tahmin ederken şeritteki `ver` kartlarını sayıyor mu, sepete bakıyor mu?
  - En sık hangi çeldiriciyi seçiyor ("verdiğim" mi, "başta vardı" mı)?
  - Soruyu atlamak istiyor mu?
- **Köşe 4 (K11):**
  - Tur sayısını bir sayı olarak mı seçiyor, "bir tur daha" diye deneyerek mi buluyor?
  - Kutunun "herkese bir" anlamını kendi sözcükleriyle söyleyebiliyor mu?
  - Sofrayı kayıp gibi mi görüyor? "Artan bal nereye gitti, kim yiyecek?" sorusuna ne cevap veriyor?
- **Değer anı:** Değer kartını dinliyor mu, geçmek mi istiyor? "Paylaşmak ne demek?" sorusuna oyundan örnek veriyor mu?
