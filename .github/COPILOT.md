# 🧠 Copilot Agent Guide for ReisingerIntelliAppV1

## 🔍 Projektübersicht
Reisinger IntelliApp V1 ist eine moderne, plattformübergreifende Mobile App auf Basis von **.NET MAUI**, mit der Reisinger IntelliDrive-Geräte gesteuert und überwacht werden können.

Die App unterstützt Verbindungen zu lokalen und cloudbasierten Geräten, bietet eine Vielzahl an Einstellungsseiten, eine Kartenansicht für das visuelle Platzieren von Geräten sowie Schnittstellen zu JSON-APIs für Gerätekonfigurationen.

## 🏗️ Architektur

- **Frontend**: .NET MAUI (XAML + C#)
- **Struktur**:
  - `Views/`: UI-Seiten (TabbedPages, Device Pages etc.)
  - `Model/`: Datenmodelle (Gerätestatus, Parameter etc.)
  - `Services/`: API-Kommunikation, JSON-Schnittstellen
  - `Controls/` und `Behaviors/`: Custom Controls & Interaktionen
- **Dateien**: PDF-Pläne werden konvertiert und in der App angezeigt.
- **Navigation**: Shell-basiert, mit dynamischen Tab-Ansichten
- **Externe Kommunikation**: JSON via REST API zu Reisinger IntelliDrive-System

## 🧑‍💻 Anforderungen an Copilot Agent

Wenn ein Issue bearbeitet wird, **soll Copilot Agent:**
1. Relevanten Code analysieren und verstehen (besonders in `Views/`, `Model/`, `Services/`)
2. Eine **vollständige Lösung** implementieren
3. Bei UI-Themen: **Visuelles Verhalten testen** (z. B. bei Zoom, Drag-and-Drop)
4. Bei Logikänderungen: sicherstellen, dass bestehende Funktionen **nicht beeinträchtigt** werden
5. Änderungen in einem **Pull Request** zusammenfassen, inkl.:
   - Kurzer Beschreibung der Lösung
   - Hinweis auf betroffene Dateien
   - ggf. TODOs für manuelles Testing

## ⚙️ Einschränkungen

- App muss **offlinefähig** bleiben
- Kompatibilität mit Android steht im Vordergrund
- Keine Backend-Datenbank – alle Daten sind temporär oder per API geladen
- Gerätepositionen auf PDF-Plänen müssen **persistierbar** und wiederherstellbar sein
- Es wird mit `Magick.NET` gearbeitet zur PDF-Konvertierung

## ✅ Teststrategie

Copilot Agent sollte beim Erstellen von Funktionen:
- Vorhandene ViewModels oder Services prüfen, ob Erweiterung sinnvoll ist
- Für neue Features: klare Abgrenzung, keine Seiteneffekte
- Bei Navigation: prüfen, ob der Shell-Stack korrekt funktioniert
- Bei Gerätekonfigurationen: JSON auf Gültigkeit prüfen

## 🧾 Beispiel-Issue-Format

```markdown
### Titel
Implementiere Drag-and-Drop für Geräte auf PDF-Bauplan

### Kontext
In der FloorPlanManagerPage sollen Geräte (ContentViews) in den Positioniermodus gebracht werden, um sie per Touch zu verschieben.

### Anforderungen
- Beim Aktivieren: ContentView leicht vergrößern, grau hinterlegen
- Drag-and-Drop via Touch
- Neue Position abspeichern
- Position auch nach Zoom/Pan erhalten bleiben

### Hinweis
- Geräte werden dynamisch aus einer Liste erstellt
- Siehe: `DevicePlacerService.cs`, `DeviceControl.xaml`
