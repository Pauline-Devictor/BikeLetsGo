package com.soc.wsclient;

import com.soap.ws.client.generated.BikeService;
import com.soap.ws.client.generated.IBikeService;

import java.util.Scanner;

public class Client {
    static BikeService bikeService = new BikeService();
    static IBikeService iBikeService = bikeService.getBasicHttpBindingIBikeService();
    static Scanner scanner = new Scanner(System.in);
    public static void main(String[] args) {
        System.out.println("Hello World! we are going to test a SOAP client written in Java");
        getDepartureAndArrivalPoint();
        //Donner addresse départ, arrivée
        /*while (true) {
            getDepartureAndArrivalPoint();
            //scanner.nextLine();
            System.out.println("Recherche d'itinéraire en cours");
            String itinerary = iBikeService.getItinerary(depart, arrivee);
            if (itinerary.equals("Addresses not found, please try again")) {
                System.out.println("Une erreur a eu lieu dans la recherche des adresses.\n Voulez vous réessayer (y/yes ou n/no) ?");
                answer = scanner.nextLine();
                if (answer.equals("n") || answer.equals("no")){
                    return;
                }
                else{
                    continue;
                }
            }

            System.out.println(iBikeService.getItinerary(depart, arrivee));

            System.out.println("Voulez vous actualiser l'itinéraire ? (y/yes ou n/no)");
            answer = scanner.nextLine();
            if (answer.equals("no") || answer.equals("n")){
                return;
            }
        }*/
    }


    static void getDepartureAndArrivalPoint(){
        System.out.println("Entrez une adresse de départ");
        //String depart = "1 Rue Pierre Scheringa, 95000 Cergy";
        //String depart = "107 Rue Cheret, 94000 Créteil";
        String depart = "place du général de gaulle, rouen";
        //String depart = "rue pelisson villeurbanne";
                    //scanner.nextLine();

        System.out.println("Entrez une adresse d'arrivée");
        String arrivee = "rue du rem martainville, rouen";
        //String arrivee = "place de la mairie, lyon";
        //String arrivee = "rue tronchet lyon";
       // String arrivee = "14 Rue des Sarrazins, 94000 Créteil";
        //String arrivee = "Boulevard d'Erkrath 22, 95650 Puiseux-Pontoise";
            //scanner.nextLine();
        System.out.println("Voulez-vous un itinéraire détaillé ? (y/yes ou n/no)");
        String detailled = scanner.nextLine();
        boolean detailledBool = false;
        if (detailled.equals("y") ||detailled.equals("yes"))
                detailledBool = true;

        lookForAnItinerary(depart,arrivee,detailledBool);
    }

    static void lookForAnItinerary(String depart, String arrivee, boolean detailled) {


        System.out.println("Recherche d'itinéraire en cours");
        String itinerary = iBikeService.getItinerary(depart, arrivee,detailled);


        System.out.println(itinerary);

        System.out.println("Voulez-vous essayer un nouvel itinéraire ? (y/yes)");
        String answer = scanner.nextLine();
        if (answer.equals("y") || answer.equals("yes"))
            getDepartureAndArrivalPoint();
    }
}
