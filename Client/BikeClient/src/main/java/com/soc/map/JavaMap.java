package com.soc.map;

import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.VirtualEarthTileFactoryInfo;
import org.jxmapviewer.cache.FileBasedLocalCache;
import org.jxmapviewer.input.CenterMapListener;
import org.jxmapviewer.input.PanKeyListener;
import org.jxmapviewer.input.PanMouseInputListener;
import org.jxmapviewer.input.ZoomMouseWheelListenerCenter;
import org.jxmapviewer.painter.CompoundPainter;
import org.jxmapviewer.painter.Painter;
import org.jxmapviewer.viewer.*;

import javax.swing.*;
import javax.swing.event.MouseInputListener;
import java.io.File;
import java.util.*;

public class JavaMap {
    public static void createMap(String itinerary){
        String[] itineraries = itinerary.split("test");
        String[] destinations = itineraries[0].split("/");
        String d = destinations[0];
        String begin = destinations[1];
        String end = destinations[2];
        String a = destinations[3];
        //System.out.println(d +" "+ begin+" "+ end+" "+ a);

        // Create a TileFactoryInfo for Virtual Earth
        TileFactoryInfo info = new VirtualEarthTileFactoryInfo(VirtualEarthTileFactoryInfo.MAP);
        DefaultTileFactory tileFactory = new DefaultTileFactory(info);

        // Setup local file cache
        File cacheDir = new File(System.getProperty("user.home") + File.separator + ".jxmapviewer2");
        tileFactory.setLocalCache(new FileBasedLocalCache(cacheDir, false));

        // Setup JXMapViewer
        JXMapViewer mapViewer = new JXMapViewer();
        mapViewer.setTileFactory(tileFactory);
        List<GeoPosition> listPositions = new ArrayList<>();
        for (String s: itineraries[1].split("/")) {
            listPositions.add(new GeoPosition(Double.parseDouble(s.split(",")[1]), Double.parseDouble(s.split(",")[0])));
        }
        GeoPosition depart = new GeoPosition(Double.parseDouble(d.split(",")[0]), Double.parseDouble(d.split(",")[1]));
        GeoPosition beginBike = new GeoPosition(Double.parseDouble(begin.split(",")[0]), Double.parseDouble(begin.split(",")[1]));
        GeoPosition endBike = new GeoPosition(Double.parseDouble(end.split(",")[0]), Double.parseDouble(end.split(",")[1]));
        GeoPosition arrivee = new GeoPosition(Double.parseDouble(a.split(",")[0]), Double.parseDouble(a.split(",")[1]));

        // Create a track from the geo-positions
        RoutePainter routePainter = new RoutePainter(listPositions);

        // Set the focus
        mapViewer.zoomToBestFit(new HashSet<>(listPositions), 0.7);

        // Create waypoints from the geo-positions
        Set<Waypoint> waypoints = new HashSet<>(Arrays.asList(
                new DefaultWaypoint(depart),
                new DefaultWaypoint(beginBike),
                new DefaultWaypoint(endBike),
                new DefaultWaypoint(arrivee)));


        // Set the focus
        mapViewer.setZoom(10);
        mapViewer.setAddressLocation(depart);

        // Add interactions
        MouseInputListener mia = new PanMouseInputListener(mapViewer);
        mapViewer.addMouseListener(mia);
        mapViewer.addMouseMotionListener(mia);
        mapViewer.addMouseListener(new CenterMapListener(mapViewer));
        mapViewer.addMouseWheelListener(new ZoomMouseWheelListenerCenter(mapViewer));
        mapViewer.addKeyListener(new PanKeyListener(mapViewer));

        // Create a waypoint painter that takes all the waypoints
        WaypointPainter<Waypoint> waypointPainter = new WaypointPainter<>();
        waypointPainter.setWaypoints(waypoints);

        mapViewer.setOverlayPainter(waypointPainter);

        // Create a compound painter that uses both the route-painter and the waypoint-painter
        List<Painter<JXMapViewer>> painters = new ArrayList<>();
        painters.add(routePainter);
        painters.add(waypointPainter);

        CompoundPainter<JXMapViewer> painter = new CompoundPainter<>(painters);
        mapViewer.setOverlayPainter(painter);

        // Display the viewer in a JFrame
        JFrame frame = new JFrame("Itinerary");
        frame.getContentPane().add(mapViewer);
        frame.setSize(800, 600);
        frame.setDefaultCloseOperation(JFrame.DISPOSE_ON_CLOSE);
        frame.setVisible(true);

    }
}

