import React from "react";
import { HashRouter, Routes, Route, Navigate } from "react-router-dom";
import PrivateRoute from "../components/PrivateRoute";
import Login from "../pages/Auth/Login";
import ProductsList from "../pages/Products/List";
import ProductDetail from "../pages/Products/Detail";
import Shops from "../pages/Shops/List";
import CategoryExplorer from "../pages/Catalog/CategoryExplorer"; // <-- EKLENDİ

export default function AppRoutes(){
  return (
    <HashRouter>
      <Routes>
        <Route path="/login" element={<Login />} />

        {/* Katalog → Kategori/Özellikler */}
        <Route
          path="/catalog"
          element={
            <PrivateRoute>
              <CategoryExplorer />
            </PrivateRoute>
          }
        />

        {/* Ürünler */}
        <Route
          path="/products"
          element={
            <PrivateRoute>
              <ProductsList/>
            </PrivateRoute>
          }
        />
        <Route
          path="/products/:id"
          element={
            <PrivateRoute>
              <ProductDetail/>
            </PrivateRoute>
          }
        />

        {/* Mağazalar */}
        <Route
          path="/shops"
          element={
            <PrivateRoute>
              <Shops/>
            </PrivateRoute>
          }
        />

        {/* Varsayılan yönlendirme: giriş yapmamışsa /login, yapmışsa /catalog da tercih edebilirsin */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </HashRouter>
  );
}
