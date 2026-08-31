<template>
  <div>
    <Menubar :model="menu">
      <template #start>
        <span class="brand-title"><i class="pi pi-moon"></i> 珍宝展厅</span>
      </template>
    </Menubar>
    <div class="page-wrap">
      <router-view />
    </div>
    <div class="text-center text-muted py-3 small">TreasureChamber 展厅产品管理系统 · 纯本地离线运行</div>
  </div>
</template>

<script setup>
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

const menu = [
  { label: '产品浏览', icon: 'pi pi-home', command: () => router.push('/') },
  { label: '产品管理', icon: 'pi pi-database', command: () => router.push('/manage') },
  { label: '批量导入', icon: 'pi pi-upload', command: () => router.push('/import') },
  { label: '二维码中心', icon: 'pi pi-qrcode', command: () => router.push('/qr') },
  { label: '意向单', icon: 'pi pi-shopping-cart', command: () => router.push('/intent-orders') },
  { label: '系统设置', icon: 'pi pi-cog', command: () => router.push('/settings') }
]
</script>
