# ISISLab.HelpDesk

<p align="center">
  <img width=200 height=auto src="https://github.com/isislab-unisa/ISISLab-Bot/blob/main/imgs/Logo.png?raw=true" alt=""/>
</p>

ISISLab.HelpDesk inizialmente fu pensato come un progetto di studio delle tecnologie Cloud di Microsoft Azure. Il suo scopo iniziare era quello di comprendere tutti i meccanismi celati dietro ai servizi Cloud offerti da Microsoft e, di conseguenza, sviluppare un Bot che potesse fungere da esempio per tali funzionalità.

Col tempo, tale progetto si è trasformato in un qualcosa di più concreto, diventando a tutti gli effetti un assistente virtuale per Discord che potesse essere utilizzato da tutti, con possibilità di espansione mediante AI.

Sono stati, in oltre, migliorati vari aspetti e funzionalità del bot, abbandonando per quanto possibile i servizi Cloud per far spazio ad una implementazione stabile, locale e con possibilità di utilizzo / estensione alla portata di tutti e con pochi semplici passaggi.

</br>

## Funzionalità del Bot

***Di seguito verranno elencate tutte le attuali interazioni possibili con il Bot***

Categoria       | Comandi
-------------   | -------------
**Calendario**  | Mostra il calendario dei seminari
**Aiuto**       | Aiuto generico
~               | Aiuto sulla tesi
~               | Aiuto sul tirocinio
**Seminari**    | Informazioni sui prossimi seminari
~               | Prenotazione di un seminario
**Social**      | Informazioni sul laboratorio
~               | Informazioni sui canali social del laboratorio

</br>

## Prerequisiti necessari

Come accennato nella descrizione, lo scopo principale di questo Bot è soprattutto la sua messa in funzione nel modo più veloce possibile. Per tener fede a ciò, abbiamo diviso i prerequisiti in due parti. La **prima parte** può essere seguita da chi non ha necessità di compilare tale Bot, ma semplicemente eseguirlo. La **seconda parte**, invece, si focalizzerà sui prerequisiti utili a sviluppare ed effettuare il deploy del Bot.

+ Prerequisiti standard (prima parte)
  + Un account Google
  + Un account Discord
  + [Docker Desktop](https://docs.docker.com/desktop/ "")

+ Prerequisiti avanzati (seconda parte)
  + [Visual Studio 2019](https://visualstudio.microsoft.com/it/vs/older-downloads/ "")
  + [.Net Core SDK 3.1](https://dotnet.microsoft.com/en-us/download/dotnet/3.1 "") (Bot Development)
  + [Conda](https://docs.conda.io/projects/conda/en/latest/user-guide/install/download.html "") (AI Development)

Per il download e l'installazione di tali requisiti, è consigliato seguire la guida ufficiale sulla base del vostro sistema operativo. Le fasi seguenti devono essere seguite da tutti (sviluppatori e non) così da avere tutto il necessario per utilizzare il Bot senza incorrere in spiacevoli errori o problemi.

</br>

## Setup del Bot e delle sue funzionalità

Prima di proseguire, è consigliato copiare ed incollare il seguente blocco di codice in un notepad, così che possiate man mano salvare le informazioni che andremo a prendere nelle varie fasi di questo setup.

```hjson
BotToken: <discord_bot_token>

AI:
{
  BaseUrl: http://<ip-address>:5000/
}

Seminars:
{
  ServerId: -<server-id>-
  ChannelId: -<channel-id>-
}

Google:
{
  Calendar:
  {
    APIKey: <api-key>
    CalendarId: <calendar-email>
  }

  Gmail:
  {
    From: <from-mail>
    To: <to-mail>
    Password: <app-password>
  }
}
```

### Creazione del Bot sulla piattaforma Discord

Navigate su questa pagina [Discord Developer Portal](https://discord.com/developers/applications "") ed accedete con le vostre credenziali. Dovreste trovarvi di fronte ad una schermata come questa:

<p align="center">
  <img src="https://github.com/isislab-unisa/ISISLab-Bot/blob/main/imgs/discord-setup/setup_1.PNG?raw=true" alt=""/>
</p>

</br>

Cliccate adesso su **New Application** e nel Popup che verrà mostrato, inserire il nome della vostra nuova applicazione.
Una volta nella schermata di configurazione, provvedete ad inserire Immagine, Descrizione ed altre informazioni utili.

<p align="center">
  <img src="https://github.com/isislab-unisa/ISISLab-Bot/blob/main/imgs/discord-setup/setup_2.PNG?raw=true" alt=""/>
</p>

</br>

Rechiamoci adesso nella sezione **Bot** nel menu laterale e nella nuova pagina che ci viene mostrata, clicchiamo su **Add Bot** confermando il popup che ci viene mostrato. Iniziamo subito premendo il pulsante **Reset Token** così che ce ne venga mostrato uno nuovo e copiamo il Token appena generato all'interno del nostro file di configurazione.

<p align="center">
  <img src="https://github.com/isislab-unisa/ISISLab-Bot/blob/main/imgs/discord-setup/setup_3.PNG?raw=true" alt=""/>
</p>

</br>

Una volta salvato il nostro Token, rechiamoci nella sezione **Privileged Gateway Intents** poco più sotto ed abilitiamo tutti i permessi mostrati in tale sezione.
* Presence Intent
* Server Members Intent
* Message Content Intent

Salviamo quindi tutti i cambiamenti effettuati fino a questo punto.

Rechiamoci adesso nella sezione **OAuth2** e navighiamo in **URL Generator**. Nella nuova finestra che ci comparirà, selezioniamo come **Scopes** la checkbox **Bot** e nella nuova lista che ci comparirà, selezioniamo **Administrator**

<p align="center">
  <img src="https://github.com/isislab-unisa/ISISLab-Bot/blob/main/imgs/discord-setup/setup_4.PNG?raw=true" alt=""/>
</p>

</br>

Una volta selezionato tutto il necessario, in basso ci comparirà un URL che ci servirà per invitare il nostro Bot all'interno del nostro server. A questo punto, potete scegliere di utilizzare tale link adesso, incollandolo nel vostro browser e selezionando il vostro server di riferimento, oppure utilizzarlo al termine di questa guida. A voi la scelta.

### Configurazione server e canale discord

Una delle funzionalità offerte dal Bot è la possibilità di notificare in un apposito canale, dentro un apposito server, i seminari che sono stati programmati. Per rendere tutto funzionante, ci occorre prendere gli **ID Unici** sia del nostro server che del nostro canale di riferimento. Di seguito verrà spiegato come fare.

Rechiamoci all'interno dell'applicazione di Discord e clicchiamo sull'ingranaggio in basso a sinistra, di fianco al nostro nome utente. Una volta nelle impostazioni, clicchiamo sul tab **Avanzate** e una volta all'interno, abilitiamo la **Modalità Sviluppatore**

<p align="center">
  <img src="https://github.com/isislab-unisa/ISISLab-Bot/blob/main/imgs/discord-setup/setup_5.PNG?raw=true" alt=""/>
</p>

</br>

Adesso, non ci resta che cliccare col tasto destro sul nostro server / canale per poter copiare l'**ID** di riferimento.

<p align="center">
  <img src="https://github.com/isislab-unisa/ISISLab-Bot/blob/main/imgs/discord-setup/setup_6.PNG?raw=true" alt=""/>
</p>

</br>

Copiati entrambi gli ID, non ci rimane che incollarli tutti nel nostro file di riferimento.
**ATTENZIONE**: Nel file di configurazione, nella sezione **Seminars** sono presenti due trattini '**-**' che devono racchiudere il nostro ID. Fate attenzione a non cancellarli poiché potreste incorrere in errori durante l'utilizzo del Bot

<p align="center">
  <img src="https://github.com/isislab-unisa/ISISLab-Bot/blob/main/imgs/discord-setup/setup_7.PNG?raw=true" alt=""/>
</p>

</br>

### Gestione Invio Mail con GMail

Un ulteriore importante aspetto del bot è senza dubbio l'invio delle mail una volta prenotato un seminario. Tale funzionalità consente una notifica istantanea nel caso di una prenotazione di un seminario, così che si possa facilmente creare un evento e finalizzare il tutto.

Per raggiungere tale obiettivo, dobbiamo definire, nella sezione dedicata a Gmail nel file di configurazione, la mail del **Mittente** (From) e la mail del **Destinatario** (To). Per fare ciò, ci basta semplicemente inserire le nostre mail all'interno dei relativi campi. Bisogna prestare attenzione ad inserire nel campo **From** una mail Google, altrimenti si rieschia di incorrere in errori. Per la sezione **To**, invece, è possibile utilizzare una qualsiasi email.

Per la sezione **Password** bisogna fare qualche passaggio in più, che verrà spiegato di seguito.
Eseguiamo il login con il nostro account Google e rechiamoci nelle impostazioni del nostro account. Una volta arrivati nelle impostazioni del nostro account, rechiamoci nella sezione **Sicurezza** e clicchiamo su **Password per le app**.

Procediamo quindi a generare una nuova Password seguendo tutti i passaggi elencati da Google. Una volta generata tale password, la possiamo incollare nell'apposita sezione nel nostro file di testo.

### Gestione API Google Calendar
