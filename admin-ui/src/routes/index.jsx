import React from "react";
import { HashRouter, Routes, Route, Navigate } from "react-router-dom";
import PrivateRoute from "../components/PrivateRoute";
import Login from "../pages/Auth/Login";
import ProductsList from "../pages/Products/List";
import ProductDetail from "../pages/Products/Detail";
import Shops from "../pages/Shops/List";

export default function AppRoutes(){
  return (
    <HashRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/products" element={<PrivateRoute><ProductsList/></PrivateRoute>} />
        <Route path="/products/:id" element={<PrivateRoute><ProductDetail/></PrivateRoute>} />
        <Route path="/shops" element={<PrivateRoute><Shops/></PrivateRoute>} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </HashRouter>
  );
}
