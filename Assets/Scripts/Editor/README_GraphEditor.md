# Graph Editor Tool

Bu Graph Editor tool'u, ThiefMaster projesi için görsel graph oluşturma ve düzenleme imkanı sağlar. Grid tabanlı bir sistem kullanarak node'lar ve edge'ler oluşturabilir, düzenleyebilir ve test edebilirsiniz.

## Özellikler

### 🎯 Temel Özellikler
- **Görsel Graph Editörü**: Grid tabanlı, zoom ve pan destekli editör
- **Node Yönetimi**: 7 farklı node tipi (Normal, Start, Goal, Breakable, Redirector, Trap, Enemy)
- **Edge Yönetimi**: 4 farklı edge tipi (Standard, Directed, Slippery, Breakable)
- **Gerçek Zamanlı Önizleme**: Edge oluştururken canlı önizleme
- **Otomatik Kaydetme**: ScriptableObject tabanlı veri saklama

### 🎨 Görsel Özellikler
- **Renk Kodlaması**: Her node ve edge tipi için farklı renkler
- **Grid Sistemi**: Ayarlanabilir grid boyutu
- **Zoom ve Pan**: Mouse wheel ile zoom, middle click ile pan
- **Seçim Vurgulaması**: Seçili elementler sarı renkte vurgulanır

## Kullanım Kılavuzu

### 1. Graph Editor'ü Açma
```
Unity Editor → Tools → Graph Editor
```

### 2. Yeni Graph Oluşturma
1. Graph Editor penceresini açın
2. "New Graph" butonuna tıklayın
3. Dosya adını ve konumunu belirleyin
4. Graph otomatik olarak seçilecektir

### 3. Node Oluşturma
- **Ctrl + Sol Click**: Boş alanda node oluşturur
- **Sağ Click → Create Node**: Context menu'den node oluşturur

### 4. Node Düzenleme
- **Sol Click**: Node seçer
- **Sağ Click → Set Type**: Node tipini değiştirir
- **Sağ Click → Delete Node**: Node'u siler
- **Inspector Panel**: Seçili node'un özelliklerini düzenler

### 5. Edge Oluşturma
- **Shift + Sol Click**: Edge oluşturmaya başlar
- **Sol Click**: Edge'i tamamlar (hedef node'a tıklayarak)
- **Sağ Click → Cancel**: Edge oluşturmayı iptal eder

### 6. Edge Düzenleme
- **Sol Click**: Edge seçer
- **Inspector Panel**: Seçili edge'in özelliklerini düzenler
- **Delete Edge**: Edge'i siler

### 7. Navigasyon
- **Mouse Wheel**: Zoom in/out
- **Ctrl + Mouse Wheel**: Daha hassas zoom
- **Middle Click + Drag**: Pan (kaydırma)
- **Grid Toggle**: Grid'i açıp kapatır

## Node Tipleri

| Tip | Renk | Açıklama |
|-----|------|----------|
| Normal | Beyaz | Standart geçiş noktası |
| Start | Yeşil | Başlangıç noktası |
| Goal | Kırmızı | Hedef noktası |
| Breakable | Sarı | Kırılabilir node |
| Redirector | Mavi | Yön değiştirici |
| Trap | Siyah | Tuzak |
| Enemy | Magenta | Düşman |

## Edge Tipleri

| Tip | Renk | Açıklama |
|-----|------|----------|
| Standard | Beyaz | İki yönlü geçiş |
| Directed | Cyan | Tek yönlü geçiş (ok işareti) |
| Slippery | Turuncu | Kaygan geçiş |
| Breakable | Kırmızı | Kırılabilir geçiş |

## Test ve Doğrulama

### GraphTestRunner Kullanımı
1. Sahnede boş bir GameObject oluşturun
2. `GraphTestRunner` component'ini ekleyin
3. Test edilecek graph'ı atayın
4. Context menu'den testleri çalıştırın

### Mevcut Testler
- **Graph Creation Test**: Graph'ın doğru oluşturulduğunu kontrol eder
- **Node Access Test**: Tüm node'lara erişilebilirliği test eder
- **Edge Validation Test**: Edge'lerin geçerliliğini kontrol eder
- **Graph Connectivity Test**: Start ve Goal node'larının varlığını kontrol eder
- **Duplicate Detection Test**: Tekrarlanan node ve edge'leri tespit eder
- **Solvability Test**: Graph'ın çözülebilir olup olmadığını kontrol eder

## Kısayollar

| Kısayol | İşlem |
|---------|-------|
| Ctrl + Sol Click | Node oluştur |
| Shift + Sol Click | Edge oluşturmaya başla |
| Sol Click | Seçim |
| Sağ Click | Context menu |
| Middle Click + Drag | Pan |
| Ctrl + Mouse Wheel | Zoom |
| Ctrl + S | Kaydet |

## Best Practices

### 1. Graph Tasarımı
- Her graph'ta en az bir Start ve bir Goal node'u bulundurun
- Node'ları mantıklı bir şekilde yerleştirin
- Edge'leri gereksiz karmaşıklık yaratmayacak şekilde çizin

### 2. Performans
- Çok büyük graph'larda zoom seviyesini düşürün
- Gereksiz node ve edge'leri silin
- Graph'ı düzenli aralıklarla test edin

### 3. Veri Yönetimi
- Graph'ları düzenli olarak kaydedin
- Yedek kopyalar oluşturun
- Test sonuçlarını kontrol edin

## Sorun Giderme

### Yaygın Sorunlar

**Graph yüklenmiyor:**
- ScriptableObject dosyasının doğru konumda olduğundan emin olun
- Unity Editor'ü yeniden başlatın

**Node'lar görünmüyor:**
- Zoom seviyesini kontrol edin
- Grid boyutunu ayarlayın
- Pan offset'ini sıfırlayın

**Edge oluşturulamıyor:**
- Kaynak ve hedef node'ların var olduğundan emin olun
- Aynı node'lar arasında birden fazla edge oluşturmayın

**Testler başarısız:**
- Graph'ın geçerli olduğundan emin olun
- Start ve Goal node'larının bulunduğunu kontrol edin
- Duplicate node/edge olmadığından emin olun

## Teknik Detaylar

### Dosya Yapısı
```
Assets/Scripts/
├── Gameplay/Graph/
│   ├── Graph.cs              # Ana graph sınıfı
│   ├── Node.cs               # Node sınıfı ve tipleri
│   ├── Edge.cs               # Edge sınıfı ve tipleri
│   ├── GraphData.cs          # Serialize edilebilir veri yapısı
│   ├── GraphScriptableObject.cs # ScriptableObject wrapper
│   └── GraphTestRunner.cs    # Test ve doğrulama aracı
└── Editor/
    └── GraphEditorWindow.cs  # Görsel editör penceresi
```

### Veri Akışı
1. **GraphScriptableObject** → Graph verilerini saklar
2. **GraphData** → Serialize edilebilir node ve edge verileri
3. **Graph** → Runtime'da kullanılan graph instance'ı
4. **Node/Edge** → Graph elementleri

### Genişletme Noktaları
- Yeni node tipleri eklemek için `NodeType` enum'unu genişletin
- Yeni edge tipleri eklemek için `EdgeType` enum'unu genişletin
- Özel testler eklemek için `GraphTestRunner`'ı genişletin
- Görsel özellikler eklemek için `GraphEditorWindow`'u genişletin

## Lisans
Bu tool ThiefMaster projesi için özel olarak geliştirilmiştir. 