namespace Nabiz.Application.Exceptions;

public abstract class UygulamaHatasi(string mesaj) : Exception(mesaj);
public class BulunamadiHatasi(string mesaj = "Kayıt bulunamadı.") : UygulamaHatasi(mesaj);
public class CakismaHatasi(string mesaj) : UygulamaHatasi(mesaj);
public class DogrulamaHatasi(string mesaj) : UygulamaHatasi(mesaj);
public class YetkisizHatasi(string mesaj = "Yetkisiz.") : UygulamaHatasi(mesaj);
