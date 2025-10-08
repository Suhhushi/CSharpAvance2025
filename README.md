# 🧩 ConvertApp

## 🧠 Format du fichier JSON attendu

Le fichier JSON doit contenir un **ou plusieurs tableaux** d’objets ayant **trois propriétés** (dans un ordre quelconque).
Chaque entrée représente un élément (par ex. un artiste ou un album).

### Exemple de JSON compatible

```json
{
  "albums": [
    { "id": "1", "name": "Thriller", "extra": "Michael Jackson" },
    { "id": "2", "name": "Back in Black", "extra": "AC/DC" },
    { "id": "3", "name": "The Dark Side of the Moon", "extra": "Pink Floyd" }
  ],
  "artists": [
    { "id": "1", "name": "Michael Jackson", "extra": "Pop" },
    { "id": "2", "name": "AC/DC", "extra": "Rock" }
  ]
}
```

⚠️ Peu importe le nom des blocs racine (`albums`, `artists`, etc.), le programme analyse **tous les tableaux** trouvés dans le JSON.

---

👤 **José Silva Costa**

