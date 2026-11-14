# PROJE TALİMATI: Windows 11 Modüler Görev Çubuğu ve Masaüstü Yönetim Sistemi

## 1. Rolün ve sorumlulukların

Sen kıdemli bir Windows masaüstü yazılım mimarı, C#/.NET geliştiricisi, WPF arayüz uzmanı, Windows API entegrasyon mühendisi, performans optimizasyon uzmanı ve yazılım test mühendisi olarak çalışacaksın.

Görevin Windows 11 üzerinde çalışan, mevcut görev çubuğuna gelişmiş bir alternatif sunan, kullanıcı tarafından tamamen özelleştirilebilen, modern, hızlı, güvenli, modüler ve uzun vadede geliştirilebilir bir masaüstü uygulaması oluşturmaktır.

Bu projeyi yalnızca çalışan bir prototip olarak değil, gerçek kullanıcıların günlük kullanabileceği, sürdürülebilir bir yazılım ürünü olarak ele al.

**Temel ilkeler:**

- Clean Architecture ve SOLID prensiplerini uygula.
- Modüler, test edilebilir ve genişletilebilir kod yaz.
- Gereksiz bağımlılık ve karmaşıklıktan kaçın.
- Windows 11 ile uyumlu, güncel ve desteklenen API'leri tercih et.
- Performans, RAM, CPU, GPU ve pil tüketimini sürekli gözet.
- Modern ve profesyonel bir arayüz oluştur.
- Güvenli hata yönetimi ve kurtarma sistemleri geliştir.
- Sistemin kararlılığını ve kullanıcının kontrolünü ön planda tut.
- Gelecekte yeni özelliklerin kolayca eklenmesini sağla.
- Geçici çözümler yerine sürdürülebilir mimari tercih et.

**Çalışma şekli:** Önce araştır, sonra mimariyi oluştur, ardından uygulamayı aşamalı olarak geliştir. Her aşamada derle, test et, hataları gider ve sonraki aşamaya geç.

---

## 2. Teknoloji yığını

Tercih edilen teknolojiler:

| Katman | Teknoloji |
|---|---|
| Programlama dili | C# |
| Framework | .NET 10 LTS |
| Masaüstü arayüzü | WPF |
| Tasarım sistemi | Windows 11 Fluent Design |
| Mimari | Clean Architecture + MVVM |
| Windows entegrasyonu | Win32 API, P/Invoke, Shell API |
| Veri saklama | SQLite |
| ORM | EF Core SQLite veya daha hafif uygun çözüm |
| Bağımlılık yönetimi | Microsoft.Extensions.DependencyInjection |
| Logging | Microsoft.Extensions.Logging + Serilog |
| MVVM araçları | CommunityToolkit.Mvvm |
| Birim testleri | xUnit |
| UI testleri | Windows UI Automation uyumlu araçlar |
| Paketleme | WiX Toolset veya uygun Windows kurulum sistemi |
| CI/CD | GitHub Actions |
| Versiyon kontrolü | Git |

Bu liste başlangıç önerisidir. Geliştirme öncesinde sürüm uyumluluğunu, güvenliği, paket boyutunu ve performansı doğrula.

WPF üzerinde Windows 11'e uygun Fluent görünümü sağlamak için yerleşik temaları ve gerekiyorsa güvenilir açık kaynak bileşenleri değerlendir.

Gereksiz üçüncü taraf UI kütüphaneleri kullanma.

---

# 3. Uygulamanın temel amacı

Windows 11'in mevcut görev çubuğundan daha esnek ve kullanışlı bir sistem geliştir.

Kullanıcı:

- Görev çubuğunu istediği gibi düzenleyebilmeli.
- Uygulamaları kategorilere ayırabilmeli.
- Klasörler oluşturabilmeli.
- Uygulamaları sürükleyip bırakabilmeli.
- Açık pencereleri yönetebilmeli.
- Görev çubuğunun görünümünü değiştirebilmeli.
- Farklı çalışma profilleri oluşturabilmeli.
- Birden fazla monitörde kullanabilmeli.
- Kısayolları ve hızlı eylemleri yönetebilmeli.
- Pil tüketimini ve uygulamanın performansını kontrol edebilmeli.

İşletim sistemiyle olabildiğince doğal bütünleşen bir deneyim sun.

## 3.1. Windows 11 entegrasyon stratejisi

Öncelikle Windows 11'in görev çubuğunun teknik sınırlarını araştır.

Şu üç yaklaşımı karşılaştır:

1. Windows'un mevcut görev çubuğuyla birlikte çalışan özel panel.
2. Mevcut görev çubuğunu kullanıcı isteğiyle gizleyip onun yerine çalışan bağımsız görev çubuğu.
3. Windows Shell üzerinde daha derin entegrasyon.

Her yaklaşım için uyumluluk, güncelleme riski, performans, güvenlik, yönetici izni ihtiyacı ve geri alma imkânlarını değerlendir.

**Önemli:** Windows 11 görev çubuğu ve bildirim alanının bütün özellikleri desteklenen genel API'lerle yeniden üretilemeyebilir. Desteklenmeyen API'leri varmış gibi kullanma.

İlk kararlı çözüm, bağımsız çalışan ve geri alınabilir bir görev çubuğu olmalıdır.

Windows Explorer işlemini sonlandırma, desteklenmeyen DLL enjeksiyonları yapma, kritik Shell dosyalarını değiştirme veya kalıcı riskli kayıt defteri düzenlemeleri kullanma.

Başlat menüsü, sistem tepsisi, bildirim merkezi ve hızlı ayarlar gibi bileşenlerde desteklenen entegrasyon sınırlarını tespit et. Tam entegrasyon mümkün değilse Windows'un orijinal bileşenlerine erişimi koruyan bir çözüm oluştur.

---

# 4. Modüler yazılım mimarisi

Uygulama monolitik, birbirine sıkı bağlı bir kod yığını olmamalıdır.

Her özellik bağımsız sorumluluklara ayrılmalıdır.

Temel mimari:

```text
Presentation
     |
Application
     |
Domain
     ^
     |
Infrastructure / WindowsIntegration
```

Bağımlılık yönü Clean Architecture kurallarına uygun olmalıdır.

Presentation katmanı doğrudan Windows API veya SQL çağrıları yapmamalıdır.

Application katmanı iş akışlarını yönetmeli, Domain katmanı iş kurallarını barındırmalıdır.

Windows'a özgü işlemler arayüzler üzerinden soyutlanmalıdır.

## 4.1. Önerilen klasör yapısı

```text
ModularTaskbar/
|
+-- src/
|   |
|   +-- ModularTaskbar.Desktop/
|   |   +-- App.xaml
|   |   +-- App.xaml.cs
|   |   +-- Bootstrapper/
|   |   +-- Views/
|   |   +-- ViewModels/
|   |   +-- Controls/
|   |   +-- Components/
|   |   +-- Behaviors/
|   |   +-- Converters/
|   |   +-- Resources/
|   |   |   +-- Styles/
|   |   |   +-- Themes/
|   |   |   +-- Icons/
|   |   |   +-- Fonts/
|   |   |   +-- Animations/
|   |   +-- Localization/
|   |   +-- Accessibility/
|   |   +-- Navigation/
|   |
|   +-- ModularTaskbar.Domain/
|   |   +-- Entities/
|   |   +-- ValueObjects/
|   |   +-- Enums/
|   |   +-- Events/
|   |   +-- Rules/
|   |   +-- Interfaces/
|   |
|   +-- ModularTaskbar.Application/
|   |   +-- Interfaces/
|   |   +-- UseCases/
|   |   +-- Services/
|   |   +-- Commands/
|   |   +-- Queries/
|   |   +-- DTOs/
|   |   +-- Validators/
|   |   +-- Mapping/
|   |
|   +-- ModularTaskbar.Infrastructure/
|   |   +-- Persistence/
|   |   |   +-- Database/
|   |   |   +-- Configurations/
|   |   |   +-- Migrations/
|   |   |   +-- Repositories/
|   |   +-- Settings/
|   |   +-- Logging/
|   |   +-- Caching/
|   |   +-- Files/
|   |   +-- Backup/
|   |   +-- Diagnostics/
|   |
|   +-- ModularTaskbar.WindowsIntegration/
|   |   +-- Native/
|   |   +-- Shell/
|   |   +-- WindowTracking/
|   |   +-- ProcessManagement/
|   |   +-- MonitorManagement/
|   |   +-- DpiManagement/
|   |   +-- PowerManagement/
|   |   +-- Hotkeys/
|   |   +-- Startup/
|   |   +-- AppBar/
|   |   +-- Notifications/
|   |
|   +-- ModularTaskbar.Modules/
|   |   +-- Taskbar/
|   |   +-- Groups/
|   |   +-- Folders/
|   |   +-- RunningApps/
|   |   +-- PinnedApps/
|   |   +-- Profiles/
|   |   +-- Search/
|   |   +-- Shortcuts/
|   |   +-- Themes/
|   |   +-- SystemWidgets/
|   |   +-- Settings/
|   |   +-- Performance/
|   |
|   +-- ModularTaskbar.Contracts/
|       +-- Modules/
|       +-- Events/
|       +-- Services/
|
+-- tests/
|   +-- ModularTaskbar.UnitTests/
|   +-- ModularTaskbar.IntegrationTests/
|   +-- ModularTaskbar.UITests/
|   +-- ModularTaskbar.PerformanceTests/
|
+-- benchmarks/
+-- tools/
+-- scripts/
+-- installer/
+-- assets/
+-- docs/
|   +-- architecture/
|   +-- features/
|   +-- api/
|   +-- decisions/
|   +-- testing/
|   +-- performance/
|   +-- troubleshooting/
|
+-- .github/
|   +-- workflows/
|
+-- .editorconfig
+-- Directory.Build.props
+-- Directory.Packages.props
+-- ModularTaskbar.sln
+-- README.md
+-- ARCHITECTURE.md
+-- ROADMAP.md
+-- CHANGELOG.md
+-- CONTRIBUTING.md
```

Bu yapı bir başlangıç taslağıdır. Gereksiz projeler veya boş klasörler oluşturma.

Klasörleri gerçek ihtiyaçlara göre aç. Özellikle modülleri bağımsızlaştırırken gereksiz proje sayısından kaçın.

Bir modül, diğer modülün iç detaylarına doğrudan bağımlı olmamalıdır.

Yeni özellik eklemek mevcut sistemi baştan yazmayı gerektirmemelidir.

---

# 5. Görev çubuğu sistemi

## 5.1. Temel görev çubuğu

Kullanıcı şu özellikleri yönetebilmelidir:

- Görev çubuğunun konumu: alt, üst, sol, sağ.
- Simge boyutları.
- Görev çubuğu yüksekliği ve genişliği.
- Ortalanmış veya hizalanmış simgeler.
- Otomatik gizleme.
- Tam ekran uygulamalarda davranış.
- Saydamlık.
- Arka plan görünümü.
- Yuvarlatılmış köşeler.
- Gruplar arasındaki boşluklar.
- Fare üzerine gelme davranışları.
- Çoklu monitör konumlandırması.
- Görev çubuğunu kilitleme.
- Görev çubuğu düzenini sıfırlama.

Windows 11'e uyumlu modern bir görünüm oluştur.

Kullanıcı isterse bölümleri birbirinden görsel olarak ayırabilmelidir.

Örneğin solda geliştirme araçları, ortada aktif uygulamalar, sağda sistem bilgileri bulunabilir.

## 5.2. Görev çubuğunu bölümlere ayırma

Görev çubuğunu sürüklenebilir ve yönetilebilir alanlardan oluştur.

Örnek:

```text
[Başlat] [Yazılım ▼] [İletişim ▼]
          [Açık Uygulamalar]
       [Sistem] [Saat] [Pil]
```

Bölümler:

- Yeni oluşturulabilmeli.
- Silinebilmeli.
- Yeniden adlandırılabilmeli.
- Sıralanabilmeli.
- Taşınabilmeli.
- Gizlenebilmeli.
- Sabitlenebilmeli.
- Ayrı görünümlerle özelleştirilebilmeli.

Kullanıcının oluşturduğu yapı yeniden başlatmadan sonra korunmalıdır.

---

# 6. Uygulama gruplandırma ve klasör sistemi

Bu projenin en önemli özelliklerinden biri dinamik gruplandırmadır.

## 6.1. Manuel gruplar

Kullanıcı istediği kadar grup oluşturabilmelidir.

Örnek gruplar:

- Yazılım Geliştirme
- İletişim
- Tarayıcılar
- Tasarım
- Oyunlar
- Ofis
- Sistem Araçları

Her grup:

- İsim
- Simge
- Renk
- Açıklama
- Sıralama
- İçerdiği uygulamalar
- Görünüm tercihi

bilgilerini taşımalıdır.

## 6.2. Klasör sistemi

Grup simgesine basıldığında modern bir açılır panel göster.

Panel içerisinde:

- Uygulama simgeleri.
- Izgara veya liste görünümü.
- Uygulama arama.
- Sürükle-bırak.
- Yeniden sıralama.
- Uygulama sabitleme.
- Klasör düzenleme.
- Sağ tıklama menüsü.

bulunmalıdır.

Klasörlerin içinde alt klasör oluşturma desteğini tasarla. İç içe klasörlerin performans ve kullanılabilirlik sınırlarını belirle.

## 6.3. Akıllı gruplandırma

Uygulamaların otomatik sınıflandırılabileceği bir kural sistemi geliştir.

Örnek:

- Visual Studio Code → Yazılım
- Windows Terminal → Yazılım
- Chrome → Tarayıcı
- Discord → İletişim

Sınıflandırma uygulama kimliği, yürütülebilir dosya ve kullanıcı kurallarına dayanmalıdır.

Kullanıcı kuralları her zaman otomatik tahminlerden öncelikli olmalıdır.

Kullanıcının onayı olmadan mevcut grup düzenini değiştirme.

---

# 7. Açık uygulama ve pencere yönetimi

Windows üzerinde çalışan kullanıcı pencerelerini takip eden bağımsız bir servis geliştir.

Araştırılabilecek API'ler:

- EnumWindows
- GetWindowThreadProcessId
- GetWindowText
- IsWindowVisible
- SetWinEventHook
- GetForegroundWindow
- SetForegroundWindow
- DWM API
- Windows Shell API

Gerekli diğer API'leri Microsoft belgelerinden doğrula.

Her API için kullanım sınırlamalarını ve hata durumlarını dikkate al.

## Özellikler

- Açık uygulamaları listeleme.
- Aktif pencereyi işaretleme.
- Uygulamalar arasında geçiş.
- Aynı uygulamanın pencerelerini gruplama.
- Pencere önizlemesi.
- Pencereyi küçültme.
- Pencereyi geri yükleme.
- Pencereyi kapatma.
- Çoklu pencere seçimi.
- Son kullanılan uygulamaları gösterme.
- Pencere durumunun canlı güncellenmesi.

Windows'un odak alma kısıtlamalarına saygı göster. Her koşulda zorla öne getirme vaat etme.

UWP ve paketlenmiş Windows uygulamalarını da değerlendir.

Arka planda çalışan fakat görünür bir penceresi olmayan sistem işlemlerini gereksiz yere göstermemelisin.

Olay tabanlı takip kullan.

Sürekli bütün işlemleri tarayan yüksek frekanslı döngüler oluşturma.

---

# 8. Sürükle-bırak ve etkileşimler

Tüm arayüzde tutarlı bir sürükle-bırak altyapısı oluştur.

Kullanıcı:

- Uygulamayı gruba taşıyabilmeli.
- Gruplar arasında aktarım yapabilmeli.
- Simge sıralamasını değiştirebilmeli.
- Yeni klasör oluşturabilmeli.
- Uygulamayı görev çubuğuna sabitleyebilmeli.
- Sabitlenmiş simgeleri kaldırabilmeli.

Etkileşimlerde görsel geri bildirim olmalıdır.

Örneğin simge taşınırken hedef alan vurgulanabilir.

Dokunmatik ekran, klavye ve touchpad etkileşimlerini de dikkate al.

Sağ tıklama menülerini bağlama duyarlı tasarla.

---

# 9. Modern arayüz ve tasarım sistemi

Arayüz Windows 11 Fluent Design yaklaşımına uygun olmalıdır.

Kullanılabilecek görsel özellikler:

- Mica ve Acrylic etkileri, platform desteklediği ölçüde.
- Modern yuvarlatılmış köşeler.
- Soft shadows.
- Akıcı ancak hafif animasyonlar.
- Hover ve focus durumları.
- Koyu ve açık tema.
- Sistem vurgu rengi.
- Modern ikonografi.
- Tutarlı spacing ve typography.

Görselliği performansın önüne koyma.

## 9.1. Tasarım token sistemi

Renkleri, boşlukları ve boyutları dağınık şekilde kod içerisine yazma.

Merkezi tasarım değişkenleri oluştur:

```text
DesignTokens/
  Colors
  Typography
  Spacing
  Radius
  Shadows
  Motion
  IconSizes
  ControlSizes
```

Tema değişikliğinde uygulamanın tüm bileşenleri tutarlı şekilde güncellenmelidir.

## 9.2. Ayarlar penceresi

Profesyonel bir ayarlar ekranı geliştir.

Bölümler:

1. Genel
2. Görev Çubuğu
3. Görünüm ve Temalar
4. Gruplar ve Klasörler
5. Uygulama Yönetimi
6. Kısayollar
7. Çoklu Monitör
8. Performans ve Pil
9. Bildirimler
10. Gelişmiş Ayarlar
11. Yedekleme ve Geri Yükleme
12. Hakkında ve Güncellemeler

Ayarların çoğu mümkünse yeniden başlatma gerektirmeden uygulanmalıdır.

---

# 10. Performans ve pil optimizasyonu

Bu proje için en kritik gereksinimlerden biri düşük sistem kaynak tüketimidir.

Uygulama, sürekli açık kalan bir sistem bileşeni gibi düşünülmelidir.

## 10.1. CPU optimizasyonu

- Event-driven architecture kullan.
- Gereksiz polling yapma.
- Pencere olaylarını gerektiğinde birleştir.
- Değişmeyen UI öğelerini tekrar oluşturma.
- Gereksiz arka plan işlemlerini engelle.
- Olay aboneliklerini doğru yönet.
- UI thread üzerinde ağır işlemler çalıştırma.
- Büyük listelerde virtualization kullan.
- Debounce ve throttle mekanizmalarını uygun olaylarda uygula.

## 10.2. RAM optimizasyonu

- Gereksiz nesne oluşturmaktan kaçın.
- Tekrarlı ikon ve görüntüleri cache'le.
- Büyük kaynakları ihtiyaç üzerine yükle.
- Görsel önizlemeleri gerektiğinde oluştur.
- Kullanılmayan kaynakları serbest bırak.
- Event handler kaynaklı memory leak sorunlarını önle.
- Uzun süreli çalışma testleri gerçekleştir.

## 10.3. GPU optimizasyonu

- Sürekli çalışan animasyonları sınırla.
- Görünmeyen bileşenleri güncelleme.
- DWM tarafından sağlanan efektleri uygun şekilde kullan.
- Gereksiz blur ve katmanlı efektlerden kaçın.
- Gölge ve saydamlık maliyetlerini ölç.

## 10.4. Akıllı güç yönetimi

Üç çalışma modu oluştur:

**Tasarruf modu**

- Gereksiz animasyonları kapat.
- Görsel efektleri azalt.
- Arka plan işlemlerini sınırla.
- Önizlemeleri ihtiyaç üzerine oluştur.

**Dengeli mod**

- Görsellik ve kaynak tüketimini dengede tut.
- Temel animasyonlar açık olabilir.
- Normal arka plan güncelleme davranışını kullan.

**Performans modu**

- Gelişmiş animasyonlar kullanılabilir.
- Görsel geçişler daha zengin olabilir.
- Önizleme özellikleri etkin olabilir.

Bu modların hiçbirinde gereksiz CPU döngüleri bulunmamalıdır.

Windows pil tasarrufu durumunu ve güç kaynağı değişikliklerini desteklenen sistem bildirimleri üzerinden izle.

Kullanıcı pilde çalışırken isteğine göre otomatik tasarruf moduna geçebilmelidir.

## 10.5. Performans hedefleri

Şu başlangıç hedeflerini ölçerek değerlendir:

- Boştayken ortalama CPU kullanımı: mümkünse %0,5 altında.
- Normal kullanımda RAM: mümkünse 150 MB altında.
- Belirgin sürekli GPU yükü oluşmaması.
- Hızlı açılış ve düşük etkileşim gecikmesi.
- Uzun süreli kullanımda bellek artışının kontrol altında olması.

Bunlar garanti değil, optimizasyon hedefleridir. Gerçek sonuçlar donanım ve test koşullarına bağlıdır.

Test metodolojisini ve donanım özelliklerini raporla.

---

# 11. Çoklu monitör ve DPI desteği

Windows 11 çoklu monitör senaryolarına uyumlu ol.

- Monitörleri algıla.
- Her monitöre ayrı görev çubuğu yerleştirebil.
- Monitöre özel ayarlar sakla.
- Farklı çözünürlüklerde doğru görüntülen.
- Farklı DPI ölçeklendirmelerini destekle.
- Monitör bağlama ve çıkarma olaylarını takip et.
- Kullanılabilir ekran alanını doğru hesapla.
- Tam ekran oyun ve uygulamaları engelleme.
- Ekran konumu değişikliklerinde düzeni koru.

Per-monitor DPI awareness davranışını test et.

Monitör bağlantısı koparsa kaybolmuş pencere veya erişilemeyen ayarlar oluşmamalıdır.

---

# 12. Profiller ve çalışma alanları

Kullanıcı farklı görev çubuğu düzenlerini profil olarak kaydedebilmeli.

Örneğin:

**Yazılım geliştirme profili**
- VS Code
- Terminal
- Tarayıcı
- Git araçları

**Günlük kullanım profili**
- Chrome
- WhatsApp
- Dosya Gezgini
- Medya uygulamaları

**Oyun profili**
- Oyun başlatıcıları
- Discord
- Performans araçları

Profiller:

- Oluşturulabilir.
- Düzenlenebilir.
- Kopyalanabilir.
- Silinebilir.
- Dışa aktarılabilir.
- İçe aktarılabilir.

Profil değişiminde görev çubuğu düzeni otomatik güncellenmelidir.

İleride belirli uygulamalara veya çalışma koşullarına göre profil değiştirme kuralları eklenebilmelidir.

---

# 13. SQLite ve veri mimarisi

Kullanıcının verilerini yerel olarak sakla.

Başlangıçta bulut hesabı zorunlu olmasın.

Önerilen veri modelleri:

```text
UserSettings
TaskbarLayouts
TaskbarSections
ApplicationGroups
GroupItems
ApplicationEntries
PinnedApplications
Profiles
ProfileLayouts
Themes
ShortcutBindings
WindowRules
MonitorConfigurations
PerformancePreferences
```

Veri modelini normalize et, ancak gereksiz tablo karmaşıklığından kaçın.

Veritabanı şeması versiyonlanmalıdır.

Migration sistemi kurulmalıdır.

Eski uygulama sürümündeki veriler yeni sürüme güvenli biçimde aktarılmalıdır.

## Veri güvenliği

- SQL parametrelerini güvenli kullan.
- Kullanıcı dosya yollarını doğrula.
- Uygulama başlatma argümanlarını kontrol et.
- Hassas ayarları gerekiyorsa Windows'un uygun koruma mekanizmalarıyla sakla.
- Veritabanı bozulmalarına karşı kurtarma stratejisi oluştur.

Kullanıcıya JSON üzerinden ayar dışa aktarma ve içe aktarma imkânı sun.

Dışa aktarılan veriler için şema sürümü bulundur.

---

# 14. Windows başlangıcı ve sistem yaşam döngüsü

Uygulama açılış ve kapanış davranışlarını güvenli yönet.

Özellikler:

- Windows başladığında otomatik başlatma.
- Otomatik başlatmayı ayarlardan kapatma.
- Tek uygulama örneğiyle çalışma.
- Güvenli kapanma.
- Beklenmeyen kapanmadan kurtarma.
- Kullanıcının orijinal Windows görev çubuğuna dönebilmesi.
- Görev çubuğunun kullanılamaz hâle gelmesini engelleyen kurtarma seçeneği.

Normal çalışma için yönetici yetkisi isteme.

Kritik işlemlerde izin gerekirse nedenini kullanıcıya açıkla.

Kullanıcı istemedikçe sistem kayıtlarını kalıcı olarak değiştirme.

Uygulama kaldırıldığında kullanıcı tercihine göre veriler temizlenebilmeli.

---

# 15. Hata yönetimi ve uygulama güvenilirliği

Merkezi bir hata yönetimi sistemi oluştur.

Hataları sınıflandır:

```text
Errors/
  ApplicationErrors
  ValidationErrors
  WindowsApiErrors
  PersistenceErrors
  ModuleErrors
  UiErrors
  RecoveryErrors
```

Bu yapı, gerçek kod yerleşimine uygun biçimde düzenlenmelidir.

## 15.1. Hata işleme

- Beklenen hatalar için anlamlı sonuç tipleri kullan.
- İstisnaları uygun katmanda yakala.
- İstisnaları sessizce yutma.
- Kullanıcıya teknik olmayan anlaşılır mesajlar göster.
- Teknik ayrıntıları log dosyalarına kaydet.
- Hatalarda ilgili modülü güvenli şekilde devre dışı bırakabil.
- Kritik olmayan hatalarda tüm uygulamayı kapatma.
- Gerçekten kritik hatalarda güvenli kapanma gerçekleştir.

Global exception handler mekanizmalarını kur.

Asenkron işlemlerde gözden kaçan exception oluşmasını önle.

## 15.2. Log sistemi

Log seviyeleri:

- Trace
- Debug
- Information
- Warning
- Error
- Critical

Log dosyaları kullanıcı profilinin uygun uygulama veri dizininde bulunmalıdır.

Log rotation ve saklama sınırları uygula.

Sürekli tekrarlanan olayların log dosyalarını şişirmesini engelle.

Kişisel kullanıcı verilerini gereksiz yere loglama.

## 15.3. Güvenli mod

Uygulama art arda açılış hatası veriyorsa güvenli mod oluştur.

Güvenli modda:

- Ek modüller kapatılır.
- Ağır görsel efektler devre dışı kalır.
- Temel ayarlara erişim sağlanır.
- Sorunlu yapılandırma geri alınabilir.
- Varsayılan düzen geri yüklenebilir.

---

# 16. Modül sistemi ve gelecekte genişletme

Yeni özellikleri kolayca ekleyebilmek için açık bir modül sözleşmesi tasarla.

Örneğin:

```text
IModule
IModuleInitializer
IModuleLifecycle
IModuleSettings
IModuleEventHandler
```

Gereksiz soyutlama oluşturma; her arayüzün gerçek kullanım amacı olsun.

Modüller mümkün olduğunca bağımsız çalışmalıdır.

Bir modülün başarısız olması diğerlerini bozmayacak şekilde hata sınırları oluştur.

Modül iletişiminde gevşek bağlı olaylar veya açık servis sözleşmeleri kullan.

Dependency Injection altyapısı üzerinden modül servislerini kaydet.

İlk aşamada dışarıdan rastgele DLL çalıştıran bir plugin sistemi oluşturma.

Öncelikle uygulama içi modülleri güvenli şekilde geliştir. Üçüncü taraf eklenti desteğini ayrıca tasarla ve güvenlik değerlendirmesine tabi tut.

## Gelecekte eklenebilecek özellikler

- Görev çubuğu widget'ları.
- Gelişmiş uygulama arama.
- Komut başlatıcı.
- Sistem kaynak göstergeleri.
- Ağ durumu.
- Gelişmiş klavye kısayolları.
- Akıllı pencere düzenleri.
- Otomasyon kuralları.
- Profil senkronizasyonu.
- Temaların dışa ve içe aktarılması.
- Eklenti altyapısı.
- Çalışma alanı şablonları.

Mimariyi bu genişlemeleri destekleyecek biçimde hazırla, fakat kullanılmayan özellikler için aşırı mühendislik yapma.

---

# 17. Kullanıcı deneyimi

Uygulama teknik bilgisi olmayan kullanıcılar için de anlaşılır olmalıdır.

UX kuralları:

- Basit ve sezgisel gezinme.
- Tutarlı menüler.
- Az sayıda işlemle hedefe ulaşma.
- Karmaşık ayarlar için açıklamalar.
- Geri alma desteği.
- Klavye ile erişim.
- Ekran okuyucu uyumluluğu.
- Focus yönetimi.
- Yüksek kontrast desteği.
- Windows ölçeklendirme desteği.
- Türkçe ve İngilizce dil desteği.

Bütün arayüz metinlerini localization kaynaklarından yönet.

Metinleri doğrudan View veya ViewModel içerisine sabitleme.

İlk açılış için kısa bir kurulum ve tanıtım akışı geliştir.

---

# 18. Test stratejisi

Testleri proje tamamlandıktan sonraya bırakma.

Her modül geliştirilirken ilgili testleri oluştur.

## Birim testleri

- Grup oluşturma.
- Klasör yönetimi.
- Profil değiştirme.
- Sıralama.
- Ayar doğrulama.
- Uygulama eşleştirme kuralları.
- Hata durumları.

## Entegrasyon testleri

- SQLite işlemleri.
- Repository katmanı.
- Windows API adaptörleri.
- Ayarların kaydedilip yüklenmesi.
- Migration işlemleri.
- Modül yaşam döngüsü.

## UI testleri

- Görev çubuğunun açılması.
- Grup menülerinin çalışması.
- Sürükle-bırak.
- Ayar ekranları.
- Tema değişimleri.
- Klavye ile kullanım.
- Farklı DPI ve monitör senaryoları.

## Performans testleri

- Boşta çalışma.
- Yoğun pencere açma-kapama.
- Çok sayıda uygulama.
- Uzun süreli kullanım.
- Pil ve priz arasında geçiş.
- Tema ve profil değiştirme.
- Monitör bağlama/çıkarma.

Ölçümleri kaydet ve mümkün olduğunda önceki sürümlerle karşılaştır.

Windows üzerinde çalıştırılmadan doğrulanamayacak testleri başarılı olarak raporlama.

---

# 19. Kod kalitesi kuralları

Aşağıdaki kurallara uy:

- Tek sorumluluk prensibi.
- SOLID.
- DRY.
- KISS.
- Clean Architecture.
- MVVM.
- Dependency Injection.
- Async/await doğru kullanımı.
- CancellationToken desteği.
- Kaynakların doğru dispose edilmesi.
- Nullable reference types.
- Anlamlı sınıf ve metot adları.
- Merkezi sabitler ve yapılandırmalar.
- Güçlü veri doğrulama.
- Test edilebilir servisler.

UI bileşenlerini iş kurallarıyla karıştırma.

Gereksiz static global state kullanma.

Magic number ve magic string kullanımını azalt.

P/Invoke tanımlarını merkezi ve güvenli bir katmanda tut.

Gerektiğinde SafeHandle ve IDisposable kullan.

Yeniden kullanılabilir bileşenler geliştir.

Kritik kodlara açıklayıcı yorumlar ekle. Anlaşılır kodu gereksiz yorumlarla doldurma.

Performans gerekçesi olmadan optimizasyon karmaşıklığı ekleme.

---

# 20. Git, sürüm yönetimi ve dokümantasyon

Proje Git ile yönetilmelidir.

Anlamlı commit mesajları kullan.

Örnek:

```text
feat(taskbar): add customizable taskbar layout
feat(groups): implement application grouping
feat(power): add adaptive power saving
fix(windows): resolve DPI positioning issue
refactor(core): decouple window tracking service
test(groups): add grouping unit tests
docs(architecture): document module boundaries
```

Her önemli değişiklikten sonra:

1. Derle.
2. Testleri çalıştır.
3. Hataları kontrol et.
4. Değişiklikleri gözden geçir.
5. Anlamlı bir commit oluştur.

Uzak depoya push etmeden önce izin iste.

README, mimari kararlar, kurulum ve geliştirme süreçleri belgelenmelidir.

Önemli mimari kararları Architecture Decision Record (ADR) olarak kaydet.

---

# 21. Geliştirme yol haritası

Projenin tamamını tek seferde kontrolsüz şekilde kodlama.

Aşağıdaki aşamaları kullan.

## Aşama 1 — Analiz ve mimari

- Çalışma ortamını incele.
- Windows ve .NET araçlarını kontrol et.
- Güncel teknik belgeleri araştır.
- Windows 11 entegrasyon sınırlarını belirle.
- Uygulama mimarisini tasarla.
- Modüllerin sorumluluklarını ayır.
- Proje ve klasör yapısını oluştur.
- Mimari dokümantasyon hazırla.

## Aşama 2 — Çalışan uygulama temeli

- WPF uygulamasını oluştur.
- MVVM altyapısını kur.
- Dependency Injection ekle.
- Logging ve error handling oluştur.
- SQLite altyapısını hazırla.
- Tema ve tasarım sistemini oluştur.
- Temel test altyapısını kur.

## Aşama 3 — Görev çubuğu

- Bağımsız görev çubuğu penceresini oluştur.
- Windows entegrasyonunu geliştir.
- Açık pencereleri algıla.
- Simge ve durum yönetimini oluştur.
- Tıklama ve pencere değiştirme işlemlerini çalıştır.
- Ekran konumlandırmasını uygula.

## Aşama 4 — Gruplar ve klasörler

- Grup oluşturma.
- Grup düzenleme.
- Uygulama ekleme.
- Klasör yapısı.
- Sürükle-bırak.
- Sıralama.
- Sağ tıklama menüleri.
- Kalıcı veri saklama.

## Aşama 5 — Gelişmiş arayüz

- Fluent görünüm.
- Tema sistemi.
- Modern açılır paneller.
- Animasyonlar.
- Ayarlar sayfası.
- Klavye erişimi.
- Türkçe/İngilizce desteği.

## Aşama 6 — Windows özellikleri

- Çoklu monitör.
- DPI yönetimi.
- Otomatik gizleme.
- Başlangıçta açılma.
- Tam ekran davranışı.
- Güvenli geri dönüş.
- Profil yönetimi.

## Aşama 7 — Performans

- CPU optimizasyonu.
- RAM optimizasyonu.
- GPU optimizasyonu.
- Pil tasarrufu modu.
- Uzun süreli çalışma testleri.
- Bellek sızıntısı analizi.
- Performans raporu.

## Aşama 8 — Kararlılık ve dağıtım

- Hata senaryolarını test et.
- Kullanıcı verilerini koru.
- Güvenli mod ve kurtarma mekanizmasını tamamla.
- Windows kurulum paketini oluştur.
- Kaldırma işlemini test et.
- Sürümleme ve changelog oluştur.
- İlk kararlı sürüm adayını hazırla.

---

# 22. Antigravity çalışma kuralları

Bu bölüm, geliştirme boyunca uyman gereken kalıcı kuralları içerir.

### Önce incele, sonra değiştir

Bir kod dosyasını değiştirmeden önce ilgili modülü, bağlantılı servisleri ve veri akışını incele.

Mevcut davranışı bozabilecek değişiklikleri belirle.

### Her özelliği uçtan uca tamamla

Bir özelliğin tamamlanması için şu parçalar çalışmalıdır:

- Domain modeli.
- İş mantığı.
- Gerekli Windows entegrasyonu.
- Veri saklama.
- Kullanıcı arayüzü.
- Hata yönetimi.
- Testler.

Yalnızca güzel görünen fakat çalışmayan bir arayüzü tamamlanmış olarak değerlendirme.

### Hataları gizleme

Derleme hatası, test hatası veya entegrasyon sorunu varsa açıkça raporla.

Sorunun nedenini araştır.

Rastgele kod silerek hatayı geçiştirme.

### Modülerlik kontrolü yap

Her önemli değişiklik sonrasında kontrol et:

- Yeni gereksiz bağımlılık oluştu mu?
- Katman sınırları ihlal edildi mi?
- Aynı kod tekrar ediyor mu?
- Servis çok fazla sorumluluk üstleniyor mu?
- Yeni özellik eklemek mevcut kodu kırıyor mu?
- Testler etkileniyor mu?

### Performansı kontrol et

Her yeni özelliğin olası CPU, RAM, GPU ve pil etkisini değerlendir.

Gereksiz zamanlayıcı, polling veya sürekli animasyon ekleme.

### Güvenliği koru

Sistem bileşenlerini riskli şekilde değiştirme.

Orijinal Windows görev çubuğuna dönme imkânını daima koru.

Test amaçlı deneysel Shell entegrasyonlarını kararlı sürüme kontrolsüz ekleme.

### Dokümantasyonu güncelle

Yeni modül veya mimari değişiklik yapıldığında ilgili dokümanları güncelle.

### Gerçek durumu raporla

Tamamlanmayan özelliği tamamlanmış gösterme.

Kodun derlenmiş olması, Windows 11 üzerinde gerçek davranışının doğrulandığı anlamına gelmez.

---

# 23. Proje takip ve görev yönetimi

Projenin kök dizininde aşağıdaki dosyaları oluştur:

```text
ROADMAP.md
TASKS.md
ARCHITECTURE.md
CHANGELOG.md
```

`TASKS.md` içerisinde her iş için:

- Görev adı.
- İlgili modül.
- Öncelik.
- Bağımlılıklar.
- Durum.
- Kabul kriterleri.
- Test durumu.

bilgilerini tut.

Görev durumları:

```text
BACKLOG
READY
IN_PROGRESS
BLOCKED
TESTING
DONE
```

Her geliştirme aşamasında bu dosyaları güncelle.

Tamamlanan görevlerin kabul kriterlerinin gerçekten karşılandığını doğrula.

---

# 24. İlk çalıştırmada yapacakların

Şimdi doğrudan geliştirme sürecine başla.

Öncelikle:

1. Mevcut çalışma klasörünü incele.
2. Daha önce proje oluşturulup oluşturulmadığını kontrol et.
3. Varsa mevcut kodları koruyarak mimari analiz yap.
4. Windows ortamını ve gerekli SDK'ları kontrol et.
5. Desteklenen Windows API ve .NET sürümlerini doğrula.
6. Teknik riskleri ve Windows 11 kısıtlamalarını belirle.
7. Nihai modüler mimariyi oluştur.
8. Proje dosyalarını ve gerçek ihtiyaç duyulan klasörleri hazırla.
9. Gerekli NuGet paketlerini kur.
10. Temel WPF uygulamasını çalışır hâle getir.
11. İlk görev çubuğu prototipini gerçek çalışan pencere algılama özelliğiyle geliştir.
12. Derleme ve testleri gerçekleştir.
13. Sonuçları ve sıradaki işleri görev dosyasına işle.

Geliştirme ortamı Windows değilse Windows'a özgü işlemleri yapılmış gibi gösterme. Mümkün olan kodları oluştur ve gerçek Windows testleri için gerekli adımları belgele.

Mevcut dosyaları izinsiz silme veya ezme.

---

# 25. Nihai başarı kriterleri

Proje şu koşulları karşılamalıdır:

- Windows 11 üzerinde gerçekten çalışır.
- Modern ve profesyonel bir arayüze sahiptir.
- Uygulamalar gruplandırılabilir.
- Klasörler oluşturulabilir.
- Açık pencereler yönetilebilir.
- Düzen sürükle-bırak ile değiştirilebilir.
- Kullanıcı ayarları kalıcıdır.
- Birden fazla monitörde çalışabilir.
- Pil tasarrufu özellikleri bulunur.
- Düşük kaynak tüketimi için optimize edilmiştir.
- Hataları kaydeder ve güvenli şekilde ele alır.
- Beklenmeyen kapanma durumunda Windows'un temel kullanımını engellemez.
- Modüler ve test edilebilir mimariye sahiptir.
- Yeni özellikler kolayca eklenebilir.
- Kurulum ve kaldırma süreçleri bulunur.
- Geliştirme ve kullanım belgeleri hazırlanmıştır.

## SON TALİMAT

Bu projede önceliğin yalnızca kod yazmak değil; doğru mimari, sürdürülebilirlik, performans, kullanıcı deneyimi ve Windows 11 uyumluluğu olmalıdır.

Tüm gereksinimleri kapsamda tut ancak aşamalı geliştir.

Önce çalışan ve güvenilir bir temel oluştur, ardından özellikleri tamamla.

Her aşamayı ölçülebilir kabul kriterleriyle doğrula.

**Şimdi proje analizine, mimari planlamaya ve ilk çalışan sürümün geliştirilmesine başla.**