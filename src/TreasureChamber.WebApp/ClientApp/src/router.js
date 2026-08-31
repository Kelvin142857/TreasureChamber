import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/', name: 'home', component: () => import('./views/BrowseHome.vue') },
  { path: '/products', name: 'products', component: () => import('./views/ProductList.vue') },
  { path: '/product/:id(\\d+)', name: 'product-detail', component: () => import('./views/ProductDetail.vue') },
  { path: '/manage', name: 'manage', component: () => import('./views/ProductManage.vue') },
  { path: '/import', name: 'import', component: () => import('./views/ImportCenter.vue') },
  { path: '/qr', name: 'qr', component: () => import('./views/QrCenter.vue') },
  { path: '/qr/print', name: 'qr-print', component: () => import('./views/QrPrint.vue'), meta: { bare: true } },
  { path: '/intent-orders', name: 'intent-orders', component: () => import('./views/IntentOrderList.vue') },
  { path: '/intent-orders/new', name: 'intent-order-create', component: () => import('./views/IntentOrderCreate.vue') },
  { path: '/intent-orders/:id(\\d+)', name: 'intent-order-detail', component: () => import('./views/IntentOrderDetail.vue') },
  { path: '/settings', name: 'settings', component: () => import('./views/Settings.vue') }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
