
# **Pixel Animator**
## Bu animatörü yaparken [AdamCYounis](https://www.youtube.com/@AdamCYounis)'ın **RetroBox** aracından esinlendim.
### *Bu araç kesinlikle bir RetroBox değil. Bazı kendine özgün özellikleri barındıran başka bir araçtır.*



Öncellikle şunu belirtmeliyim ki bu animatör en verimli 2D Pixel Art animasyonlar için kullanılabilir. Zira başka bir tarzda hiç test etmedim.

Pixel Animator, **kare kare** animasyonu ile **BoxCollider2D**'ları senkronize etmek için Unity dahilinde geliştirdiğim bir araç. Ve yanında bi kaç ek özellik daha var.

------------------

## **Pixel Animator Neyi Çözüyor?**
* Bu [videoda](https://www.youtube.com/watch?v=nBkiSJ5z-hE) gösterildiği gibi Unity'nin dahili animatöründeki ağ karmaşıklığından kaçmanıza olanak sağlar.
* BoxCollider2D objeleriyle kare kare animasyonunuzla tam nedensel bağ kurmanızı sağlar.
* GUI tarafından ayarlayabilceğiniz UnityEvent benzeri bir veri yapısıyla animasyonunuza event(UnityEvent'den çok daha esnek) ekleyip animasyonunuzu kodunuz ile tam senkron edebilirsiniz.

Etkisini daha iyi anlayabilmek için demo projesine bakabilirsiniz:
[demo]()



------------------

------------------

## **Başlarken**

### **Kurulum**
Bu url'yi kullanarak [Unity Package olarak indirebilirsiniz](https://docs.unity3d.com/Manual/upm-ui-giturl.html):
```
https://github.com/biinci/PixelAnimator.git
```
Belki ilerde Unity'nin Asset Store'unada koyabilirim.

------------------
### **Kullanım**
İlk önce *AssetMenu>Create>PixelAnimation>New Animation* yolunu izleyerek Pixel Animation objesini oluşturmanız gerekir.

[//]: # (<img src="https://github.com/biinci/PixelAnimator/blob/main/GIFs/Create_PixelAnimation.gif" width="350" height="400" />)




İlk *Pixel Animation* objenizi oluşturduktan sonra animatörü kullanmak için, kullanmak istediğiniz *sahne objesine* **PixelAnimator** bileşenini eklemelisiniz. 

[//]: # (![]&#40;https://github.com/biinci/PixelAnimator/blob/main/GIFs/Add_Animator_Component.gif&#41;)








Umarım pixel art animasyonlarınızda yardımcı olur. 


*Hangi animatör sizin için daha rahatsa onu kullanmanız çok daha iyi.*