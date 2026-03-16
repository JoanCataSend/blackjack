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
                values[i] = 1;
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
                finalMessage.text = "Déjalo ya, no hay dinero.";
                return;
            }

            apuestaActual = apuesta;
            banca -= apuesta;
            ActualizarTextoBanca();
            finalMessage.text = $"Has apostado {apuesta} euros.";

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
        CardHand dealerHand = dealer.GetComponent<CardHand>();
        CardHand playerHand = player.GetComponent<CardHand>();

        if (dealerHand.cards.Count < 2)
            return;

        int cartasRestantes = values.Length - cardIndex;

        if (cartasRestantes <= 0)
        {
            probMessage.text =
                "Probabilidades:\n" +
                "Dealer > Jugador: 0.0000\n" +
                "Si pides y quedas entre 17 y 21: 0.0000\n" +
                "Si pides y te pasas de 21: 0.0000";
            return;
        }

        int playerPoints = playerHand.points;
        int valorVisibleDealer = dealerHand.cards[1].GetComponent<CardModel>().value;

        int dealerMejor = 0;
        int jugador17a21 = 0;
        int jugadorSePasa = 0;

        for (int i = cardIndex; i < values.Length; i++)
        {
            int posibleCarta = values[i];

            int puntosPosiblesDealer = CalcularPuntosDosCartas(valorVisibleDealer, posibleCarta);
            if (puntosPosiblesDealer > playerPoints && puntosPosiblesDealer <= 21)
                dealerMejor++;

            int puntosPosiblesJugador = CalcularPuntosManoMasCarta(playerHand, posibleCarta);

            if (puntosPosiblesJugador >= 17 && puntosPosiblesJugador <= 21)
                jugador17a21++;

            if (puntosPosiblesJugador > 21)
                jugadorSePasa++;
        }

        float probDealer = (float)dealerMejor / cartasRestantes;
        float prob1721 = (float)jugador17a21 / cartasRestantes;
        float probMas21 = (float)jugadorSePasa / cartasRestantes;

        probMessage.text =
            "Probabilidades:\n" +
            $"Dealer > Jugador: {probDealer:0.0000}\n" +
            $"Si pides y quedas entre 17 y 21: {prob1721:0.0000}\n" +
            $"Si pides y te pasas de 21: {probMas21:0.0000}";
    }

    private int CalcularPuntosDosCartas(int valor1, int valor2)
    {
        int suma = 0;
        int ases = 0;

        if (valor1 == 1)
            ases++;
        else
            suma += valor1;

        if (valor2 == 1)
            ases++;
        else
            suma += valor2;

        for (int i = 0; i < ases; i++)
        {
            if (suma + 11 <= 21)
                suma += 11;
            else
                suma += 1;
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

            if (valor == 1)
                ases++;
            else
                suma += valor;
        }

        if (nuevaCarta == 1)
            ases++;
        else
            suma += nuevaCarta;

        for (int i = 0; i < ases; i++)
        {
            if (suma + 11 <= 21)
                suma += 11;
            else
                suma += 1;
        }

        return suma;
    }

    void PushDealer()
    {
        if (cardIndex >= values.Length) return;

        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
    }

    void PushPlayer()
    {
        if (cardIndex >= values.Length) return;

        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;

        int puntosJugador = player.GetComponent<CardHand>().points;
        TextoPuntosJugador.text = $"Puntos Jugador: {puntosJugador}";
    }

    public void Hit()
    {
        if (partidaTerminada)
            return;

        PushPlayer();

        int playerPoints = player.GetComponent<CardHand>().points;

        if (playerPoints > 21)
        {
            finalMessage.text = "El jugador pierde!";
            partidaTerminada = true;
            RevelarDealer();
            ActualizarTextoBanca();
            CalculateProbabilities();
            return;
        }

        CalculateProbabilities();
    }

    public void Stand()
    {
        if (partidaTerminada)
            return;

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
            finalMessage.text = "El jugador gana!";
            banca += apuestaActual * 2;
        }
        else if (playerPoints < dealerPoints)
        {
            finalMessage.text = "El dealer gana!";
        }
        else
        {
            finalMessage.text = "Empate!";
            banca += apuestaActual;
        }

        partidaTerminada = true;
        ActualizarTextoBanca();
        hitButton.interactable = false;
        stickButton.interactable = false;
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
    }

    void ComprobarBlackjack()
    {
        int dealerPoints = dealer.GetComponent<CardHand>().points;
        int playerPoints = player.GetComponent<CardHand>().points;

        if (playerPoints == 21 && dealerPoints != 21)
        {
            finalMessage.text = "¡Jugador tiene Blackjack!";
            banca += apuestaActual * 2;
            RevelarDealer();
            partidaTerminada = true;
        }
        else if (dealerPoints == 21 && playerPoints != 21)
        {
            finalMessage.text = "¡Dealer tiene Blackjack!";
            RevelarDealer();
            partidaTerminada = true;
        }
        else if (dealerPoints == 21 && playerPoints == 21)
        {
            finalMessage.text = "Empate!";
            banca += apuestaActual;
            RevelarDealer();
            partidaTerminada = true;
        }

        ActualizarTextoBanca();

        if (partidaTerminada)
        {
            hitButton.interactable = false;
            stickButton.interactable = false;
            CalculateProbabilities();
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