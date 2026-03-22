using UnityEngine;
using UnityEngine.UI;
using System;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;

    public GameObject dealer;
    public GameObject player;

    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;

    public Text finalMessage;
    public Text probMessage;

    public Text TextoPuntosJugador;
    public Text TextoPuntosDealer;

    public int[] values = new int[52];
    int cardIndex = 0;

    public int banca = 1000;
    public int apuestaActual = 0;

    public Text textoBanca;
    public Text textoApuesta;
    public InputField inputApuesta;
    public GameObject botonApostar;

    private bool partidaTerminada = false;
    private bool dealerRevelado = false;

    private void Awake()
    {
        InitCardValues();
    }

    private void Start()
    {
        ShuffleCards();
        ActualizarTextoBanca();
        ActualizarTextoApuesta();

        finalMessage.text = "Haz tu apuesta para comenzar.";
        probMessage.text = "";
        TextoPuntosJugador.text = "Puntos Jugador: 0";
        TextoPuntosDealer.text = "Puntos Dealer: ?";

        hitButton.interactable = false;
        stickButton.interactable = false;
    }

    private void InitCardValues()
    {
        for (int i = 0; i < values.Length; i++)
        {
            int rango = i % 13;

            if (rango == 0)
                values[i] = 1; // As
            else if (rango >= 1 && rango <= 9)
                values[i] = rango + 1;
            else
                values[i] = 10;
        }
    }

    private void ShuffleCards()
    {
        for (int i = 51; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            (faces[i], faces[j]) = (faces[j], faces[i]);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }

    public void HacerApuesta()
    {
        if (!partidaTerminada &&
            (player.GetComponent<CardHand>().cards.Count > 0 || dealer.GetComponent<CardHand>().cards.Count > 0))
        {
            finalMessage.text = "Termina la partida actual o pulsa Play Again.";
            return;
        }

        if (int.TryParse(inputApuesta.text, out int apuesta))
        {
            if (apuesta <= 0)
            {
                finalMessage.text = "Introduce una apuesta válida.";
                return;
            }

            if (apuesta % 10 != 0)
            {
                finalMessage.text = "La apuesta debe ser múltiplo de 10.";
                return;
            }

            if (apuesta > banca)
            {
                finalMessage.text = "No tienes suficiente dinero.";
                return;
            }

            apuestaActual = apuesta;
            banca -= apuesta;

            ActualizarTextoBanca();
            ActualizarTextoApuesta();

            finalMessage.text = $"Has apostado {apuesta}€.";

            hitButton.interactable = true;
            stickButton.interactable = true;

            StartGame();
            botonApostar.SetActive(false);
        }
        else
        {
            finalMessage.text = "Introduce una apuesta válida.";
        }
    }

    private void ActualizarTextoBanca()
    {
        textoBanca.text = $"Banca: {banca}€";
    }

    private void ActualizarTextoApuesta()
    {
        if (textoApuesta != null)
            textoApuesta.text = $"Apuesta: {apuestaActual}€";
    }

    void StartGame()
    {
        partidaTerminada = false;
        dealerRevelado = false;

        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();

        TextoPuntosDealer.text = "Puntos Dealer: ?";
        TextoPuntosJugador.text = "Puntos Jugador: 0";
        finalMessage.text = "";
        probMessage.text = "";

        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        ComprobarBlackjack();

        if (!partidaTerminada)
            CalculateProbabilities();
    }

    private void CalculateProbabilities()
    {
        if (partidaTerminada)
        {
            probMessage.text = "";
            return;
        }

        CardHand dealerHand = dealer.GetComponent<CardHand>();
        CardHand playerHand = player.GetComponent<CardHand>();

        if (dealerHand.cards.Count < 2)
            return;

        int playerPoints = playerHand.points;
        int cartasRestantes = 52 - cardIndex;

        if (cartasRestantes <= 0)
            return;

        int valorVisibleDealer = dealerHand.cards[1].GetComponent<CardModel>().value;

        float dealerMejor = 0f;
        float jugador17a21 = 0f;
        float jugadorSePasa = 0f;

        for (int i = cardIndex; i < values.Length; i++)
        {
            int posibleCarta = values[i];

            int puntosDealer = CalcularPuntosDosCartas(valorVisibleDealer, posibleCarta);
            if (puntosDealer > playerPoints && puntosDealer <= 21)
                dealerMejor++;

            int puntosJugador = CalcularPuntosManoMasCarta(playerHand, posibleCarta);

            if (puntosJugador >= 17 && puntosJugador <= 21)
                jugador17a21++;

            if (puntosJugador > 21)
                jugadorSePasa++;
        }

        float probDealer = (dealerMejor / cartasRestantes) * 100f;
        float prob1721 = (jugador17a21 / cartasRestantes) * 100f;
        float probMas21 = (jugadorSePasa / cartasRestantes) * 100f;

        probMessage.text =
            $"Dealer > Jugador: {probDealer:F2}%\n" +
            $"17<=x<=21: {prob1721:F2}%\n" +
            $"x>21: {probMas21:F2}%";
    }

    private int CalcularPuntosDosCartas(int valor1, int valor2)
    {
        int suma = 0;
        int ases = 0;

        if (valor1 == 1) ases++; else suma += valor1;
        if (valor2 == 1) ases++; else suma += valor2;

        for (int i = 0; i < ases; i++)
        {
            if (suma + 11 <= 21) suma += 11;
            else suma += 1;
        }

        return suma;
    }

    private int CalcularPuntosManoMasCarta(CardHand mano, int nuevaCarta)
    {
        int suma = 0;
        int ases = 0;

        foreach (GameObject carta in mano.cards)
        {
            int valor = carta.GetComponent<CardModel>().value;

            if (valor == 1) ases++;
            else suma += valor;
        }

        if (nuevaCarta == 1) ases++;
        else suma += nuevaCarta;

        for (int i = 0; i < ases; i++)
        {
            if (suma + 11 <= 21) suma += 11;
            else suma += 1;
        }

        return suma;
    }

    void PushDealer()
    {
        if (cardIndex >= values.Length)
        {
            ShuffleCards();
            cardIndex = 0;
        }

        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
    }

    void PushPlayer()
    {
        if (cardIndex >= values.Length)
        {
            ShuffleCards();
            cardIndex = 0;
        }

        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;

        int puntosJugador = player.GetComponent<CardHand>().points;
        TextoPuntosJugador.text = $"Puntos Jugador: {puntosJugador}";
    }

    public void Hit()
    {
        if (partidaTerminada) return;

        PushPlayer();

        int playerPoints = player.GetComponent<CardHand>().points;

        if (playerPoints > 21)
        {
            finalMessage.text = $"Te pasaste. Pierdes {apuestaActual}€.";
            partidaTerminada = true;
            RevelarDealer();
            FinalizarPartida();
            return;
        }

        CalculateProbabilities();
    }

    public void Stand()
    {
        if (partidaTerminada) return;

        RevelarDealer();

        int dealerPoints = dealer.GetComponent<CardHand>().points;

        while (dealerPoints <= 16)
        {
            PushDealer();
            dealerPoints = dealer.GetComponent<CardHand>().points;
        }

        TextoPuntosDealer.text = $"Puntos Dealer: {dealerPoints}";

        int playerPoints = player.GetComponent<CardHand>().points;

        if (dealerPoints > 21 || playerPoints > dealerPoints)
        {
            finalMessage.text = $"¡Ganaste {apuestaActual * 2}€!";
            banca += apuestaActual * 2;
        }
        else if (playerPoints < dealerPoints)
        {
            finalMessage.text = $"Pierdes {apuestaActual}€.";
        }
        else
        {
            finalMessage.text = "Empate.";
            banca += apuestaActual;
        }

        FinalizarPartida();
    }

    private void FinalizarPartida()
    {
        partidaTerminada = true;
        ActualizarTextoBanca();
        ActualizarTextoApuesta();

        hitButton.interactable = false;
        stickButton.interactable = false;

        botonApostar.SetActive(true);

        if (banca <= 0)
        {
            finalMessage.text = "Te has quedado sin dinero 💸";
            botonApostar.SetActive(false);
        }

        CalculateProbabilities();
    }

    public void PlayAgain()
    {
        partidaTerminada = false;
        dealerRevelado = false;
        apuestaActual = 0;

        TextoPuntosDealer.text = "Puntos Dealer: ?";
        TextoPuntosJugador.text = "Puntos Jugador: 0";
        finalMessage.text = "Haz tu apuesta para comenzar.";
        probMessage.text = "";

        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();

        cardIndex = 0;
        ShuffleCards();

        hitButton.interactable = false;
        stickButton.interactable = false;

        botonApostar.SetActive(true);
        inputApuesta.text = "";

        ActualizarTextoApuesta();
    }

    void ComprobarBlackjack()
    {
        int dealerPoints = dealer.GetComponent<CardHand>().points;
        int playerPoints = player.GetComponent<CardHand>().points;

        if (playerPoints == 21 && dealerPoints != 21)
        {
            finalMessage.text = "¡Blackjack! Ganas automáticamente.";
            banca += apuestaActual * 2;
            RevelarDealer();
            FinalizarPartida();
        }
        else if (dealerPoints == 21 && playerPoints != 21)
        {
            finalMessage.text = "El dealer tiene Blackjack.";
            RevelarDealer();
            FinalizarPartida();
        }
        else if (dealerPoints == 21 && playerPoints == 21)
        {
            finalMessage.text = "Empate con Blackjack.";
            banca += apuestaActual;
            RevelarDealer();
            FinalizarPartida();
        }
    }

    private void RevelarDealer()
    {
        if (!dealerRevelado)
        {
            dealer.GetComponent<CardHand>().InitialToggle();
            dealerRevelado = true;
        }

        TextoPuntosDealer.text = $"Puntos Dealer: {dealer.GetComponent<CardHand>().points}";
    }
}