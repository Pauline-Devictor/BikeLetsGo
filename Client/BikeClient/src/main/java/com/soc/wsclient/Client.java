package com.soc.wsclient;

import com.soap.ws.client.generated.BikeService;
import com.soap.ws.client.generated.IBikeService;
import com.soc.map.JavaMap;

import java.util.*;

public class Client {
    static BikeService bikeService = new BikeService();
    static IBikeService iBikeService = bikeService.getBasicHttpBindingIBikeService();
    static Scanner scanner = new Scanner(System.in);
    public static void main(String[] args) {
        System.out.println("Hello World! we are going to test a SOAP client written in Java");
        getDepartureAndArrivalPoint();
    }


    static void getDepartureAndArrivalPoint(){
        System.out.println("Entrez une adresse de départ");
        String depart = scanner.nextLine();

        System.out.println("Entrez une adresse d'arrivée");
        String arrivee = scanner.nextLine();

        System.out.println("Voulez-vous un itinéraire détaillé ? (Oui : y/yes ; Non : Entrez autre chose)");
        String detailled = scanner.nextLine();

        boolean detailledBool = detailled.equals("y") || detailled.equals("yes");
        lookForAnItinerary(depart,arrivee,detailledBool);
    }

    static void lookForAnItinerary(String depart, String arrivee, boolean detailledBool) {

        System.out.println("Recherche d'itinéraire en cours");
        String itinerary = iBikeService.getItinerary(depart, arrivee,detailledBool);
        String[] itineraries = itinerary.split("stop");

        analyseAnswer(itineraries);

        System.out.println("Voulez-vous essayer un nouvel itinéraire ? (Oui : y/yes ; Non : Entrez autre chose)");
        String answer = scanner.nextLine();
        if (answer.equals("y") || answer.equals("yes"))
            getDepartureAndArrivalPoint();
    }

    static void analyseAnswer(String[] itineraries){
        if(itineraries.length < 2){
            System.out.println(itineraries[0]);
        }else{
            System.out.println(itineraries[1]);
            JavaMap.createMap(itineraries[0]);
        }
    }
}
