# **Szenario Kontoverwaltung**

* legeBenutzerkontoFürTraineeAn(name: String, email: String, password: String, start: DateTime, end: DateTime): user User
* weiseMentorTraineeZu(mentor: Mentor, trainee: Trainee)
* schließeKonto(user User): void
* setzteInitialPassword(User user): void
* anmeldenBenutzer(name: String, email: String, password: String): boolean
* legeBenutzerkontoFürMentor/AdminAn(name: String, email: String, password: String): user User
* EntferneTraineementorZuweisung(mentor: Mentor, trainee: Trainee): void

# Szenario Lehrplan

* speichereLehrplan(Lehrplan) : Lehrplan
* passeReihenfolgeAn(Trainee, Lehrplan)

# Szenario Lektionsbearbeitung

* starteLektion(trainee: Trainee, lektion: Lektion) : Lektion
* einreichenLektion(trainee: Trainee, lektion: Lektion) : Lektion
* akzeptiereLektion(mentor: Mentor, lektion: Lektion) : Lektion
* lehneLektionAb(lektion: Lektion, mentor: Mentor, begruendung: String)
* übersetzeBearbeitungszustand(lektionId, rated)

# Statistik & Zeiterfassung

* importiereArbeitszieten(email: User-Email)
* vermerkeBearbeitungspause(trainee: User, start: DateTime, end: DateTime)

# **Szenario Wochenplan**

* ermittleWochenplan(trainee: Trainee) : Wochenplan
* setzeAktivenTrainee(trainee: trainee)

# Szenario Feedback

* speichereFeedback(feedbackdaten, traineeId, lektionId) : Feedback
* bearbeiteFeedback(feedbackdaten, traineeId, lektionId)