<br/>
<p align="center">
  <a href="https://github.com/biinci/PixelAnimator">
    <picture>
      <img width="180" alt="Pixel Animator logo" src="https://raw.githubusercontent.com/biinci/PixelAnimator/refs/heads/main/Github%20Resources/PixelAnimatorIcon.png?token=GHSAT0AAAAAAC7JURKOE52FWTIPPXBLKI3WZ5XQVDA">
    </picture>
  </a>
</p>
<p align="center">
  <a href="https://unity.com/releases/editor/whats-new/2023.2.20#installs"><img src="https://img.shields.io/badge/Unity-2023.2.7f1-blue?logo=unity" alt="Unity"></a>
  <a href="https://unity.com/releases/editor/archive"><img src="https://img.shields.io/badge/Unity-6000.0.23f1%2b-blue?logo=unity" alt="Unity"></a>

</p>
<br/>




Pixel Animator
------------------

## Bu animatörü yaparken [AdamCYounis](https://www.youtube.com/@AdamCYounis)'ın **RetroBox** aracından esinlendim.
<span style="color:gray"> ### *Bu araç kesinlikle bir RetroBox değil fakat benzer bir UI tasarımına sahiptir*</span>




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

### **Kullanım**
İlk önce *AssetMenu>Create>PixelAnimation>New Animation* yolunu izleyerek Pixel Animation objesini oluşturmanız gerekir.  
Sonra kullanacağınız sprite'ları *Pixel Sprites* başlığına sürüklemelisiniz.  
Ardından Animation nesnenizi özelleştirmek için *Window>PixelAnimator* yolundan pencereyi açın.  
Pencereden nesnenizi temel özelleştirme seçenekleri:
* Hitbox ekleme
* Sprite veya Hitbox'a özel fonksiyon ekleme

Hitbox'lara fonksiyon eklemek için 3 seçeneğiniz var. Bunlar; OnEnter, OnStay ve OnExit.

Animatörü kullanmak için, kullanmak istediğiniz *sahne objesine* **PixelAnimator** bileşenini eklemelisiniz.
SpriteRenderer bileşenini animatöre referans olarak vermeyi unutmayın  
Ardından animatörün ve oluşturduğunuz animasyon objenizin referansını alarak Play fonksiyonu ile
```
pixelAnimator.Play(animation);
```
animasyonunuzu çalıştırabilirsiniz.

[//]: # (![]&#40;https://github.com/biinci/PixelAnimator/blob/main/GIFs/Add_Animator_Component.gif&#41;)


Umarım pixel art animasyonlarınızda yardımcı olur.


*Hangi animatör sizin için daha rahatsa onu kullanmanız çok daha iyi.*


Katkıda Bulunmak İçin
------------------
