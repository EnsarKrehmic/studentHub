<a href="https://ptf.unze.ba/">
  <img width="100%" height="auto" src="https://ptf.unze.ba/wp/wp-content/uploads/2018/02/Logo-PTF018.png"/>
</a>

<h1 align="center">🎓 Informacioni sistem visokoškolske ustanove</h1>
<h3 align="center">Diplomski rad — Politehnički fakultet na Univerzitetu u Zenici</h3>

<p align="center">
  <a href="https://dotnet.microsoft.com/"><img alt=".NET" src="https://img.shields.io/badge/.NET-6+-purple?style=flat-square&logo=dotnet"></a>
  <a href="https://www.microsoft.com/sql-server"><img alt="SQL Server" src="https://img.shields.io/badge/SQL_Server-2022-red?style=flat-square&logo=microsoftsqlserver"></a>
  <a href="https://www.docker.com/"><img alt="Docker" src="https://img.shields.io/badge/Docker-Compose-blue?style=flat-square&logo=docker"></a>
  <a href="https://github.com/"><img alt="Git" src="https://img.shields.io/badge/Git-Version_Control-orange?style=flat-square&logo=git"></a>
</p>

---

## 👨‍🎓 Autor

- **Ensar Krehmić, Dipl. ing. softverskog inženjerstva**  
- 📧 [ensar.krehmic.22@size.ba](mailto:ensar.krehmic.22@size.ba)

## 👨‍🏫 Mentorstvo

- Profesor: **dr. sc. Denis Čeke**  
  📧 [denis.ceke@unze.ba](mailto:denis.ceke@unze.ba)

- Asistent: **Ehlimana Krupalija**  
  📧 [ehlimana.krupalija@unze.ba](mailto:ehlimana.krupalija@unze.ba)

---

## 📌 Opis projekta

Ovaj projekat je izrađen u sklopu **diplomskog rada** i predstavlja informacioni sistem visokoškolske ustanove.  
Cilj sistema je **digitalizacija i optimizacija** administrativnih i nastavnih procesa kroz centralizovanu web-platformu.  

Sistem pruža:  
- ✅ Jednostavnije upravljanje korisnicima, rasporedima i ispitima  
- ✅ Evidenciju ocjena i prisustva (QR/PIN kod)  
- ✅ Support panel sa AI chatbotom i direktnim upitima službi  
- ✅ Accessibility panel (kontrast, skaliranje fonta, čitač ekrana)  
- ✅ Detaljnu statistiku i analitiku akademskih procesa  
- ✅ Sigurnost kroz role-based autorizaciju, audit logove i arhiviranje  

---

## 🛠 Tehnologije

- **Frontend & Backend**: ASP.NET Core (C#), Razor Pages, Bootstrap / Tailwind CSS  
- **ORM**: Entity Framework Core  
- **Baza podataka**: Microsoft SQL Server  
- **Mobilna aplikacija (prototip)**: .NET MAUI  
- **Deploy**: Docker & Docker Compose  

---

## 🚀 Funkcionalnosti

👤 **Studenti**  
- Prijava i odjava ispita  
- Pregled ocjena i prisustva putem koda  
- Dashboard sa personalizovanim obavijestima  

🎓 **Profesori / Asistenti**  
- Evidencija prisustva  
- Kreiranje i upravljanje nastavnim aktivnostima  
- Unos i administracija ocjena  

📑 **Studentska služba**  
- Upravljanje korisnicima  
- Organizacija ispita i dodjela predmeta  
- Generisanje rasporeda i provjera konflikata  
- Administracija zahtjeva i komunikacija sa studentima  

🌐 **Gost korisnik**  
- Pregled javnih informacija i obavijesti  

---

## 📸 Pregled sistema

👉 Pogledajte sve screenshotove i funkcionalnosti sistema u posebnom fajlu:  
[SCREENSHOTS.md](./SCREENSHOTS.md)


---

## ⚙️ Zahtjevi sistema

- .NET 6 ili noviji  
- SQL Server 2022  
- Docker i Docker Compose  
- Git  

---

## ▶️ Pokretanje

Pokreni iz root direktorija:  

```bash
docker-compose up --build
```

- Web aplikacija dostupna na: `http://localhost:8080`  
- Baza podataka dostupna na: `localhost,1400`

---

**Detaljna uputstva za deploy se nalaze u fajlu [`DEPLOY.md`](./DEPLOY.md)**

---

## 🧭 Dalji razvoj

- Logičko arhiviranje korisnika i podataka (IsActive flag)
- Optimizacijski algoritmi za generisanje rasporeda (graph coloring / ILP)
- Online forme za upis i smanjenje papirologije
- Dodatne statistike (biblioteka, analitika korištenja)
- GDPR i pravne procedure za brisanje podataka

---

## 📜 Licenca

© 2025 — Ensar Krehmić | Politehnički fakultet UNZE  
Sva prava zadržana.
