# DEPLOY VODIČ — Informacijski sistem fakulteta

Ovaj dokument opisuje kako korisnik može pokrenuti kompletnu aplikaciju (web aplikaciju i bazu podataka) pomoću Docker okruženja i `docker-compose`.

---

## Preduslovi

Prije nego što započnete, potrebno je da imate instalirano:

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/) — za kloniranje repozitorija

---

## Kloniranje repozitorija

```bash
git clone https://github.com/ensarkrehmic/studenthub.git
cd studenthub
```

---

## Pokretanje sistema pomoću docker-compose

U root direktoriju projekta nalazi se datoteka `docker-compose.yml` koja omogućava jednostavno pokretanje svih komponenti sistema.

Pokrenite sljedeću komandu:

```bash
docker-compose up --build
```

Ova komanda će:

- izgraditi aplikaciju koristeći `Dockerfile`
- pokrenuti ASP.NET Core aplikaciju
- pokrenuti Microsoft SQL Server bazu u posebnom kontejneru

---

## Adrese i pristup

- Web aplikacija: [`http://localhost:8080`](http://localhost:8080)  
- Baza podataka (MS SQL Server): `localhost,1400`

---

## Konekcija prema bazi

Konekcija između aplikacije i baze je već unaprijed postavljena putem `appsettings.json` datoteke:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=localhost,1400;Initial Catalog=db_ab188_krehmiicjr3;User Id=sa;Password=A&VeryComplex123Password;TrustServerCertificate=True"
}
```

---

## Napomena o `.tar` fajlovima

Docker `.tar` fajlovi (image eksporti) **nisu uključeni** u ovaj repozitorij jer:

- Predstavljaju velike binarne fajlove
- Lako se rekreiraju lokalno korištenjem `docker-compose`
- Nisu potrebni za pokretanje sistema iz koda

---

## Kontakt

Za dodatne informacije, kontaktirajte autora putem GitHub profila ili emaila navedenog u projektu.
