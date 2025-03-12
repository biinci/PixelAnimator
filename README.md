<br/>
<p align="center">
  <a href="https://github.com/biinci/PixelAnimator">
    <picture>
      <img width="180" alt="Pixel Animator logo" src="https://github.com/user-attachments/assets/bfd74fa0-289d-459a-9a33-fec06b99d303">
    </picture>
  </a>
</p>
<p align="center">
  <a href="https://unity.com/releases/editor/whats-new/2023.2.20#installs"><img src="https://img.shields.io/badge/Unity-2023.2.7f1%2b-blue?logo=unity" alt="Unity"></a>
<!--   <a href="https://unity.com/releases/editor/archive"><img src="https://img.shields.io/badge/Unity-6000.0.23f1%2b-blue?logo=unity" alt="Unity"></a> -->

</p>
<br/>



Pixel Animator
------------------

## Bu animatörü yaparken [AdamCYounis](https://www.youtube.com/@AdamCYounis)'ın **RetroBox** aracından esinlendim.
> Bu araç kesinlikle bir RetroBox değil fakat benzer bir UI tasarımına sahiptir</em></h3>





Öncellikle şunu belirtmeliyim ki bu animatör en verimli 2D Pixel Art animasyonlar için kullanılabilir. Zira başka bir tarzda hiç test etmedim.

Pixel Animator, **kare kare** animasyonu ile **BoxCollider2D**'ları ve event'leri senkronize etmek için Unity dahilinde geliştirdiğim bir araçtır.



## **Pixel Animator Neyi Çözüyor?**


* Bu [videoda](https://www.youtube.com/watch?v=nBkiSJ5z-hE) gösterilen Unity'nin dahili animatöründeki ağ karmaşıklığından kaçmanıza olanak sağlar.
* BoxCollider2D objeleriyle kare kare animasyonunuzla tam nedensel bağ kurmanızı sağlar.
* GUI tarafından ayarlayabilceğiniz UnityEvent benzeri bir veri yapısıyla animasyonunuza event ekleyip (UnityEvent'den çok daha esnek) animasyonunuzu kodunuz ile tam senkron edebilirsiniz.

Etkisini daha iyi anlayabilmek için demo projesine bakabilirsiniz:
[demo]() (yakında)


## **Başlarken**



### **Kurulum**
Bu url'yi kullanarak [Unity Package olarak indirebilirsiniz](https://docs.unity3d.com/Manual/upm-ui-giturl.html):
```
https://github.com/biinci/PixelAnimator.git
```

### **Nesneler**

* PixelAnimation (Scriptable Object)  
* PixelAnimationController (Scriptable Object)  
* PixelAnimator (MonoBehaviour)  

Animator bu 3 temel nesne ile çalışır. PixelAnimation ile animasyonunuzu oluşturur ve PixelAnimator ile bu animasyonu oynatırsınız.
PixelAnimationController ise performans arttırmak için kullanılır(ilerde başka amaçlar getirilebilir). Bu nesne ile animasyonlarınızı gruplayabilir ve bir animatorde hangi animasyonların kullanılcağını önceden belirlemiş olursunuz, bu da performans için önemlidir.


### **Kullanım**

* ***AssetMenu>Create>PixelAnimator>New Animation*** yolunu izleyerek Pixel Animation nesnesini oluşturun.
  * Kullanacağınız sprite'ları PixelAnimation'daki ***Pixel Sprites*** başlığına sürükleyin.
* Animasyonlarınızı özelleştirmek için ***Window>PixelAnimator*** yolundan pencereyi açın.
  * Sprite tabanlı event eklemek istiyorsanız (animator, o sprite'a geldiğide event çalışır) sol üst köşeden Sprite kısma gelin
    * "+" ile Event ekleyin. İlk değişken, hangi tip bileşeni kullancağınızı belirler. İkinci değişken ise seçilen bileşenin içindeki hangi fonksiyonun çalışacağını belirler. 
  * Eğer BoxCollıder2D kullanacaksanız:
    * Timeline'daki Sol üstteki burger menu'den *Go to preferences* seçeneğine tıklayın.
    * Buradan kutu tiplerinin özelliklerini ayarlayın, seçenekleriniz:
      *  Kutu tipinin rengi (sadece editörde)
      *  Kutu tipinin ismi (hem editörde hem runtime'da)
      *  Kutu tipinin hangi [Layer](https://docs.unity3d.com/Manual/Layers.html)'da olacağı, BoxCollider2D'in gameobject'tini etkiler. (sadece runtime'da)
      *  Kutu tipinde eğer olacaksa hangi [PhysicsMaterial2D](https://docs.unity3d.com/Manual/class-PhysicsMaterial2D.html)'nin kullanılcağı, BoxCollider2D'lerin *Material* etkiler. (sadece runtime'da)
    * Tekrar burger menu'den animasyonunuza **kutu grubu** ekleyin (verileri Preferences kısmındaki kutu tipinden alır).
    * **Kutu grubunun** üzerindeki butonlarla grubu özelleştirin.
    * Kutulara event eklemek istiyorsanız sol üst köşeden *Hitbox* kısmını açın. Event eklemek için 3 seçeneğiniz var, bunlar; OnEnter, OnStay ve OnExit.
      * **Kutu grubunun** _isTrigger_ özelliğine göre event ekleyebilirsiniz. Eğer **isTrigger** özelliği açıksa ilk paremetresi **Collider2D** olan, kapalıysa **Collision2D** olan fonksiyonları ekleyebilirsiniz.
* Animasyonunuzu kullanmak için bir gameobject'e **PixelAnimator** bileşenini ekleyin.
  * SpriteRenderer bileşenini animatöre referans olarak verin.
  * Animatörün ve oluşturduğunuz animasyon objenizin referansını alarak Play fonksiyonu ile animasyonunuzu çalıştırın. `pixelAnimator.Play(idle)`


* *AssetMenu>Create>PixelAnimator>New Animation Controller* yolunu izleyerek Pixel Animation Controller nesnesini oluşturun.

Umarım frame by frame animasyonlarınızda yardımcı olur.


Katkıda Bulunmak İçin
------------------
