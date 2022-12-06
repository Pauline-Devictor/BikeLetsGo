
package com.soap.ws.client.generated;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElementRef;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Classe Java pour anonymous complex type.
 * 
 * <p>Le fragment de schéma suivant indique le contenu attendu figurant dans cette classe.
 * 
 * <pre>
 * &lt;complexType&gt;
 *   &lt;complexContent&gt;
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType"&gt;
 *       &lt;sequence&gt;
 *         &lt;element name="departure" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
 *         &lt;element name="arrival" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
 *         &lt;element name="detailled" type="{http://www.w3.org/2001/XMLSchema}boolean" minOccurs="0"/&gt;
 *       &lt;/sequence&gt;
 *     &lt;/restriction&gt;
 *   &lt;/complexContent&gt;
 * &lt;/complexType&gt;
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "departure",
    "arrival",
    "detailled"
})
@XmlRootElement(name = "getItinerary")
public class GetItinerary {

    @XmlElementRef(name = "departure", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> departure;
    @XmlElementRef(name = "arrival", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> arrival;
    protected Boolean detailled;

    /**
     * Obtient la valeur de la propriété departure.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getDeparture() {
        return departure;
    }

    /**
     * Définit la valeur de la propriété departure.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setDeparture(JAXBElement<String> value) {
        this.departure = value;
    }

    /**
     * Obtient la valeur de la propriété arrival.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getArrival() {
        return arrival;
    }

    /**
     * Définit la valeur de la propriété arrival.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setArrival(JAXBElement<String> value) {
        this.arrival = value;
    }

    /**
     * Obtient la valeur de la propriété detailled.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean isDetailled() {
        return detailled;
    }

    /**
     * Définit la valeur de la propriété detailled.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setDetailled(Boolean value) {
        this.detailled = value;
    }

}
