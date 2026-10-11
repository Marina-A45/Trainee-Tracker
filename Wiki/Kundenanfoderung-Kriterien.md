# Verwaltung von Konten und Rollen (Patrick)

<table>
<tr>
<td>Zustand</td>
<td>Requirement-Level</td>
<td>Anforderung</td>
</tr>
<tr>
<td>Done</td>
<td>MUST</td>
<td>Das System kann von mehreren Benutzern gleichzeitig verwendet werden. Es müssen keine Vorkehrungen für den Fall getroffen werden, dass zwei Benutzer den gleichen Datensatz zur gleichen Zeit bearbeiten.</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Jeder Benutzer hat eine Rolle. Verfügbare Rollen sind _Admin_, _Trainee_ oder _Mentor_.
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Trainees werden ein oder mehreren Mentoren zugewiesen werden.

Mentoren können grundsätzlich nur die Daten ihrer zugewiesenen Trainees einsehen und bearbeiten.

Die Zuweisung wird von einem Admin vorgenommen. Admins selbst haben keine solche Zuweisung und können die Daten aller Benutzer uneingeschränkt einsehen und bearbeiten.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

An Benutzern mit der Rolle _Trainee_ muss Start- und Enddatum ihres Trainee-Programms vermerkt werden.
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>Alle Benutzer werden zentral über einen Admin angelegt. Es gibt keine öffentliche Benutzerregistrierung.</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>Passwörter werden nicht im Klartext gespeichert.</td>
</tr>
<tr>
<td>Done</td>
<td>MAY</td>
<td>

Admins können für neue Benutzerkonten einen Link (URL) kopieren, mit dem der Benutzer ein Initialpasswort setzen kann.

Die Zustellung des Links an den Benutzer erfolgt außerhalb des Systems.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Admins können Benutzerkonten als _geschlossen_ markieren.

Benutzer mit geschlossenen Konten können sich nicht mehr im System anmelden.

Benutzer mit geschlossenen Konten werden nur noch in der Benutzerverwaltung angezeigt. In allen anderen Bildschirmen, Listen, etc. werden geschlossene Benutzer ausgeblendet.

 
</td>
</tr>
<tr>
<td>Not Admin Task/not Done</td>
<td>MAY</td>
<td>

Die Abteilungen Webentwicklungen und DevOps haben je ein eigenes Trainee-Programm mit eigenen Lehrplänen. Jeder Trainee <ins>und Mentor</ins> wird eindeutig einem Lehrplan zugeordnet.
</td>
</tr>
</table>

 

 

 

# Import des Lehrplans (Lukas/ Carl/ Dennis)

<table>
<tr>
<td>Zustand</td>
<td>Requirement-Level</td>
<td>Anforderung</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>Jede Lektion besteht aus einem Titel, einem geschätzten Richtaufwand (in Personentagen, also ca. 8 Stunden) sowie einer URL zum Inhalt im Cards-Deck.</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Mentoren können eine [JSON-Datei mit Lektionsdaten](https://sopro.makandra.de/curriculum-cards.json) in den Trainee-Tracker importieren.
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Nach Änderungen an einem Lehrplan wird die JSON-Datei aus dem externen [Cards-Deck](https://makandracards.com/curriculum) von einem Mentor erneut exportiert. Die JSON-Datei kann dann in den Trainee-Tracker erneut importiert werden.

Beim Neu-Import der JSON-Datei müssen die [Bearbeitungszustände vorhandener Lektionen](https://sopro.makandra.de/#states) erhalten bleiben.

Neue Lektionen, sowie Änderungen an vorhandenen Lektionen, erscheinen auch bei Trainees, die mit dem Programm bereits begonnen haben.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>Die Eigenschaft { id } in der JSON-Datei identifiziert eine Lektion eindeutig, auch über mehrere Import-Läufe hinweg. Alle anderen Eigenschaften einer Lektion können sich zwischen Import-Läufen ändern.</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Lektionen, die nicht mehr gelehrt werden, werden in der JSON-Datei als deprecated markiert. Auch kann es vorkommen, dass Lektionen aus einem früheren Import nicht mehr in einem neuen Import enthalten sind. Beide Fälle nennen wir im Folgenden _inaktive_ Lektionen.

Inaktive Lektionen werden Trainees nicht mehr zum Bearbeitungsstart angeboten. Hatte der Trainee jedoch die Bearbeitung einer Lektionen bereits begonnen, bevor diese inaktiv wurde, gilt die Lektion nur für diesen Trainee weiterhin als aktiv.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Die Reihenfolge der Lektionen ist relevant. Sie ergibt sich aus der Reihenfolge der Datensätze in der Importdatei.

Bei einem Neu-Import kann sich auch die Reihenfolge der Lektionen ändern.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MAY</td>
<td>Es gibt mehrere Lehrpläne. Beim Import wird abgefragt, welcher Lehrplan aktualisiert werden soll.</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Trainees können ~~offene~~ Lektionen ihres Lehrplans in der vom Import definierten Reihenfolge einsehen.

Auch wenn ein Lehrplan üblicherweise in der vorgegebenen Reihenfolge abgearbeitet wird, darf der Trainee jederzeit alle Lektionen einsehen und in beliebiger Reihenfolge bearbeiten.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>Bei einem teilweise bearbeiteten Lehrplan werden die Lektionen so sortiert, dass zuerst Lektionen mit Handlungsbedarf erscheinen. Danach folgen die demnächst anstehende Lektionen.</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>Die Ansicht einer Lektion zeigt alle importierten Daten, sowie einen Hyperlink zum Inhalt im Cards-Deck.</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Der Mentor kann die Reihenfolge des Lehrplans für einzelne Trainees individualisieren.

Die Standard-Reihenfolge der importierten Daten werden durch die Anpassung nicht verändert, und gelten weiterhin für neue Trainees.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Trainee und Mentor halten den Fortschritt des Trainees in der Anwendung fest.

Jede Lektion befindet sich in einem der folgenden Bearbeitungszustände: open, started, finished, accepted, rejected, skipped rated

 
</td>
</tr>
<tr>
<td>Done </td>
<td>MUST</td>
<td>

Trainees dürfen die Bearbeitungszustände nur entlang der **rot** markierten Übergänge wechseln.

Mentoren dürfen den Bearbeitungszustand einer Lektion für ~~jeden Trainee~~ <ins>zugewiesene Trainees</ins> beliebig ändern.
</td>
</tr>
<tr>
<td>Done </td>
<td>MUST</td>
<td>Die Anwendung kann von mehreren Trainees parallel verwendet werden. Die Zustände der Lektionen müssen entsprechend für jeden Trainee individuell gespeichert werden.</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Trainees können nur ihre eigenen Bearbeitungszustände einsehen und verändern.

Mentoren können die Bearbeitungszustände ~~aller~~ <ins>ihrer </ins>[<ins>zugewiesenen</ins>](https://sopro.makandra.de/#trainee-zuweisung%22) Trainees einsehen und verändern.

<ins>Admins können den Lehrplan aller Trainees einsehen.</ins>

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Es dürfen mehrere Lektionen gleichzeitig in Bearbeitung sein.

Dies kommt z. B. vor, wenn ein Trainee bei einer Lektion auf Rückmeldung des Mentors wartet, und in der Wartezeit bereits die nächste Lektion beginnt.

 
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Beim Ablehnen einer Lektion muss der Mentor einen Grund angeben. Es kann mehrzeiliger Text eingegeben werden.

Der Ablehnungsgrund ist für den Trainee und alle Mentoren einsehbar.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>Abgelehnte Lektionen werden dem Trainee zur Überarbeitung angeboten. Schließt der Trainee die Überarbeitung ab, springt sie zurück in den Zustand finished und kann beim nächsten Termin mit dem Mentor neu durchgesprochen werden.</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

[Inaktive Lektionen](https://sopro.makandra.de/#inaktive-lektionen) erscheinen für einen Trainee nur dann, wenn der Trainee die Bearbeitung schon gestartet hatte bevor die Lektion als inaktiv markiert wurde.
</td>
</tr>
<tr>
<td>Missing </td>
<td>MAY</td>
<td>

Lektionen zeigen einen Button _"AI mentor"_.

Der Button kopiert einen Prompt in die Zwischenablage, der (paraphrasiert) folgendes erreichen soll:

* Das LLM soll den Lektionstext lesen (via URL) und sich auch mit verlinkten Ressourcen vertraut machen.
* Das LLM soll dem Junior-Entwickler mit Konzepten, klärenden Fragen und Vorschlägen für nächste Schritte helfen.
* Das LLM soll keine Komplettlösungen bereit stellen. Insbesondere soll es keine vollständigen Programmteile generieren.

Ein geeigneter Prompt soll im Rahmen dieses Projektes erarbeitet und getestet werden.

Der Benutzer erhält nach Klick eine Mitteilung, dass sich der Prompt jetzt in der Zwischenablage befindet.

 
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>Während Besprechungen mit einem Trainee möchte der Mentor die Informationen anderer Trainees in der Anwendung ausblenden können.</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>Die Auswahl des Trainees wird über mehrere Interaktionen oder Bildschirmwechsel hinweg persistiert.</td>
</tr>
</table>

 

# Feedback (Carl)

<table>
<tr>
<td>Zustand</td>
<td>Requirement-Level</td>
<td>Anforderung</td>
</tr>
<tr>
<td> DONE</td>
<td>MUST</td>
<td>

Wenn eine Lektion vom Mentor erfolgreich abgenommen wurde (Zustand accepted), bittet die Anwendung den Trainee um Feedback für die Lektion.

Zum Feedback soll der (subjektiv empfundene) Schwierigkeitsgrad der Lektion, die Vorkenntnisse des Trainees, der tatsächliche Aufwand sowie eine Freitext-Anmerkung notiert werden können.

Nach Abgabe von Feedback hat die ~~Karte~~ <ins>Lektion</ins> den Bearbeitungszustand rated.

 
</td>
</tr>
<tr>
<td> DONE</td>
<td>SHOULD</td>
<td>

Es ist möglich, bei einzelnen Feedback-Kriterien keine Angabe zu machen, oder sogar ein komplett leeres Feedback abzugeben.

Auch in diesem Fall erhält die Lektion den Bearbeitungszustand rated.

 
</td>
</tr>
<tr>
<td> DONE</td>
<td>MUST</td>
<td>

Trainees können ihr eigenes Feedback einsehen, ändern und löschen

Trainees können das Feedback anderer Trainees einsehen, aber nicht ändern und löschen

Mentoren können das Feedback aller Trainees einsehen, <ins>aber nur Feedback ihrer </ins>[<ins>zugewiesenen</ins>](https://sopro.makandra.de/#trainee-zuweisung%22)<ins> Trainees</ins> ändern und löschen.

<ins>Admins können können das Feedback aller Trainees einsehen, ändern und löschen</ins>.

 
</td>
</tr>
<tr>
<td> DONE</td>
<td>MUST</td>
<td>

Die Mentoren sprechen einmal wöchentlich über das neueste Feedback. Dafür benötigen die Mentoren eine Ansicht der neuesten Feedback-Einträge über alle Trainees und Lektionen hinweg.

Eine Liste von Feedbacks für nur einen Trainee oder nur eine Lektion ist nicht nötig oder hilfreich.

 
</td>
</tr>
<tr>
<td> DONE</td>
<td>MAY</td>
<td>Eine Ansicht der Feedback-Einträge nur für eine vorher gewählte Lektion kann, muss aber nicht implementiert werden.</td>
</tr>
</table>

 

# Fortschrittskontrolle (Tim/Marina)

<table>
<tr>
<td>Zustand</td>
<td>Requirement-Level</td>
<td>Anforderung</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Folgende Kennzahlen werden berechnet:

Anwesend, Geschafft, Offen, Puffer, Geschwindigkeit, Puffer vorhersage.

Für die Schätzung der geschafften Lektionen multiplizieren wir den Richtaufwand einer Lektion mit einem Faktor, der vom Bearbeitungszustand abhängt: …
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Trainees können nur ihre eigenen Kennzahlen einsehen.

Mentoren können die Kennzahlen aller [zugewiesenen](https://sopro.makandra.de/#trainee-zuweisung%22) Trainees einsehen.

Admins können die Kennzahlen aller Trainees einsehen.

 
</td>
</tr>
<tr>
<td> </td>
<td>MAY</td>
<td>Es gibt eine grafische Darstellung der Entwicklung der Kennzahlen über Zeit (z. B. Burn-Down-Chart der noch zu bearbeitenden Personentage).</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>Lektionen im Zustand skipped, sowie inaktive Lektionen im Zustand open, fließen in keine der Kennzahlen ein.</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Die Anwesenheitszeiten einzelner Trainees (im Folgenden auch "Arbeitssstunden") wird von einem Zeiterfassungssystem abgerufen.

Zu Testzwecken stellt makandra einen API-Endpoint bereit. Dieser gibt für einen gegebenen Mitarbeiter und Zeitraum die Arbeitsstunden je Arbeitstag zurück.

Der API-Endpoint kann mit einem GET-Request an die untenstehende URL aufgerufen werden:…

 
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Die Statistik berücksichtigt die tatsächlich geleistete (bzw. erwartete) Arbeitszeit in Stunden.

Für den laufenden Tag und für künftige Tage wird die _erwartete_ Arbeitszeit zurückgegeben. Dies entspricht der regulären Arbeitszeit aus dem Anstellungsvertrag. <ins>In der Regel sind das glatte Stundenbeträge, wie 8.0 oder 4.0.</ins>

Für vergangene Tage gibt das Arbeitszeitsystem die _tatsächlich geleisteten_ Stunden zurück, wie sie der Mitarbeitende eingetragen hat. <ins>In der Regel sind das _keine_ glatten Stundenbeträge, sondern Werte wie 8.13 oder 7.58.</ins>

Bei bekannten Abwesenheiten (Urlaub, Krankheit, Wochenende) wird die tatsächliche und erwartete Arbeitszeit mit 0.0 zurückgegeben.

Ein Personentag ist mit 8 Stunden zu verrechnen.

 
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Das System erkennt Änderungen der Arbeitszeiten innerhalb von (maximal) 2 Stunden automatisch. Insbesondere erkennt es, wenn die erwarteten Arbeitsstunden (zurückgegeben für zukünftige Tage) durch die tatsächlich gestempelten Arbeitszeiten ersetzt werden (zurückgegeben für vergangene Tage).

Gelegentlich ändern sich die erfasste Arbeitszeit noch im Nachhinein. Der Grund sind Buchungsfehler und händische Korrekturen. Das System muss Änderungen der Vergangenheit bis zu 2 Wochen nach dem jeweiligen Arbeitstag berücksichtigen.

 
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Antwortet die API der Zeiterfassung mit einem anderen Status-Code als 200 OK, ist dies als Fehler zu sehen, z. B.:

HTTP/1.1 404 Not Found\
Content-Type: text/plain\
\
Unknown user

Zusätzlich ist es auch denkbar, dass die API während Wartungsarbeiten gar nicht erreichbar ist ("offline").

Für den Fall, dass keine Arbeitszeiten abgerufen werden können, meldet die Statistik-Anzeige, dass es Probleme bei der Kommunikation mit der Zeiterfassung gab. Es werden dann nur Kennzahlen angezeigt, welche ohne die Anzahl der Arbeitsstunden errechenbar sind. Bei einem späteren Abruf der Statistik wird die API-Abfrage dann erneut versucht.

 
</td>
</tr>
<tr>
<td> </td>
<td>SHOULD</td>
<td>Die Statistik-Anzeige ist voll benutzbar, während das Zeiterfassungssystems temporär offline ist. In dem Fall werden die zuletzt bekannten Kennzahlen angezeigt.</td>
</tr>
<tr>
<td> </td>
<td>SHOULD</td>
<td>

Der aktuelle Puffer wird dem Trainee an prominenter Stelle angezeigt (z. B. im Anwendungslayout).

Die Anzeige erscheint erst nach 15 anwesenden Personentagen, da vorher keine aussagekräftige Messung möglich ist.

Die Puffer-Anzeige darf keine übermäßige Last auf dem Zeiterfassungssystem erzeugen. Insbesondere darf nicht jeder Seitenabruf eine synchrone API-Anfrage auslösen. Das System muss verwendbar bleiben, während das Zeitsystem temporär offline ist.

 
</td>
</tr>
<tr>
<td> </td>
<td>SHOULD</td>
<td>Administratoren können an einem Trainee-Konto eine Bearbeitungspause vermerken (Start- und Enddatum). In diesem Fall zählt die Fortschrittsstatistik die pausierten Tage nicht mit. Es sind dann mehrere Requests an das Zeiterfassungssystem nötig.</td>
</tr>
</table>

 

# Wochenplan (Marina)

<table>
<tr>
<td>Zustand</td>
<td>Requirement-Level</td>
<td>Anforderung</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Die Liste der aktuellen und kommenden Lektionen ist als Wochenkalender (Mo-Fr) darstellbar.

Die Darstellung soll als grobe Projektion dienen. Primär soll gezeigt werden, an welchem Tag voraussichtlich welche Lektionen bearbeitet werden. Nicht wichtig ist hingegen, wie viele Stunden die Lektionen an einem Tag konkret einnehmen. Auch konkrete Uhrzeiten müssen nicht dargestellt werden.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>SHOULD</td>
<td>Der Wochenplan öffnet in der aktuellen Woche und zeigt mindestens die kommenden 6 Wochen an.</td>
</tr>
<tr>
<td> </td>
<td>MAY</td>
<td>Die Projektion ist bis zum Ende des Trainee-Programms erweiterbar (ggf. mit Paginierung).</td>
</tr>
<tr>
<td> Done</td>
<td>SHOULD</td>
<td>

Der Wochenplan nutzt für zukünftige Tage die erwarteten Arbeitszeiten, wie sie aus dem Zeiterfassungssystem abgerufen werden.

Tage ohne Sollzeit (also abwesend) sind ausgegraut dargestellt.

 
</td>
</tr>
<tr>
<td> Missing</td>
<td>MAY</td>
<td>

Der Wochenplan zeigt für die Vergangenheit, an welchen Tagen sich der Trainee mit welchen Lektionen befasst hat.

Für diese Historie werden die Zeitpunkte der [Status-Übergänge](https://sopro.makandra.de/#states) der einzelnen Lektionen herangezogen. Die Arbeitszeiten des Trainees sind hier nicht relevant.

 
</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Der Wochenplan berücksichtigt eine [individualisierte Reihenfolge der Lektionen](https://sopro.makandra.de/#individuelle-reihenfolge).
</td>
</tr>
</table>

 

# 9.UI/UX

<table>
<tr>
<td>Zustand</td>
<td>Requirement-Level</td>
<td>Anforderung</td>
</tr>
<tr>
<td> DONE</td>
<td>MUST</td>
<td>

Beim Einsehen und Bearbeiten von Lektionen geht der Listenkontext (z. B. Scroll-Positionen, Paginierung) nicht verloren. Dies gilt für alle Benutzer.

_Beispiel: Ein Trainee scrollt im Lehrplan nach unten, öffnet die Details einer Lektion und kehrt dann zum Lehrplan zurück. Der Lehrplan sollte nun an die gleiche Position gescrollt sein wie zuvor._

 
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>Formulare validieren Ihre Eingaben. Invalide Felder werden im Formular mit einer erklärenden Fehlermeldung hervorgehoben.</td>
</tr>
<tr>
<td> Done</td>
<td>SHOULD</td>
<td>

Die Benutzeroberfläche sollte im [Corporate Design von makandra](https://cd.makandra.de/) gehalten sein, insb. in Bezug auf Schriftart und Farben.

Das Logo von makandra sollte im Anwendungslayout zu sehen sein.

 
</td>
</tr>
<tr>
<td> Missing</td>
<td>MAY</td>
<td>Die Anwendung kann, muss aber nicht auf Mobiltelefonen nutzbar sein. Die Verwendung wird in erster Linie auf Notebooks und Desktop-PCs stattfinden.</td>
</tr>
<tr>
<td> Done</td>
<td>SHOULD</td>
<td>Die Benutzeroberfläche sollte in Englisch gehalten sein.</td>
</tr>
</table>

 

# Weitere Anforderungen

<table>
<tr>
<td>Zustand</td>
<td>Requirement-Level</td>
<td>Anforderung</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>

Das System muss als Web-Anwendung implementiert sein und keine Installation beim Endnutzer erfordern.

Die Web-Anwendung darf server-seitig, client-seitig oder [hybrid](https://unpoly.com/) gerendert werden.

 
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>Die Anwendung muss in Chrome (Version 145 oder höher) ohne Einschränkungen nutzbar sein.</td>
</tr>
<tr>
<td> Done</td>
<td>MUST</td>
<td>Im Rahmen des Softwareprojekts soll die Anwendung in C# und .NET Core 10 umgesetzt werden.</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Die korrekte Funktionsweise der wichtigsten Klassen und Methoden muss über Unit-Tests sichergestellt werden.

Mindestens müssen Unit Tests für den [Import](https://sopro.makandra.de/#import) sowie für die Berechnung der [Kennzahlen für die Fortschrittskontrolle](https://sopro.makandra.de/#kennzahlen) vorhanden sein.

Eine darüber hinausgehende Testabdeckung ist wünschenswert, aber nicht gefordert.

 
</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>

Die korrekte Funktionsweise der wichtigsten Interaktionen muss über End-to-End-Tests sichergestellt werden (z. B. Browser-Automatisierung mit Selenium).

Mindestens müssen End-to-End-Tests für alle Interaktionen vorhanden sein, in denen der Bearbeitungsstand einer Karte einen [Zustandsübergang](https://sopro.makandra.de/#transitions) erfährt. Dabei ist es ausreichend, wenn die Interaktion nur aus Sicht der Rolle getestet wird, welche üblicherweise den Übergang vornimmt. Z. B. ist es üblicherweise der Trainee, der eine Karte auf finished schaltet.

Eine darüber hinausgehende Testabdeckung ist wünschenswert, aber nicht gefordert.

Es genügt, wenn die End-to-End-Tests mit einem Browsertyp laufen (z. B. Chrome).

 
</td>
</tr>
<tr>
<td> Done</td>
<td>SHOULD</td>
<td>Alle vom System verwendeten Komponenten sollen Open-Source-Komponenten sein, welche permissiven Lizenzen unterliegen (kein oder schwaches Copyleft).</td>
</tr>
<tr>
<td> </td>
<td>MUST</td>
<td>Jeglicher persistenter Zustand der Anwendung muss in einer relationalen Datenbank oder einem anderen Storage-Dienst gespeichert werden. Die Anwendungsprozesse selbst sollten keinen Zustand halten, um eine horizontale Skalierung durch Hinzufügen weiterer Anwendungsserver zu ermöglichen.</td>
</tr>
</table>

 