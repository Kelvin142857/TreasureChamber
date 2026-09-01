<template>
  <div>
    <Menubar :model="menu">
      <template #start>
        <span class="brand-title"><i class="pi pi-moon"></i> 珍宝展厅</span>
      </template>
      <template #end>
        <Button :icon="isDark ? 'pi pi-sun' : 'pi pi-moon'" text rounded
                :title="isDark ? '切换到浅色主题' : '切换到深色主题'"
                @click="toggle" />
      </template>
    </Menubar>
    <div class="page-wrap">
      <router-view />
    </div>
    <div class="text-center text-muted py-3 small">TreasureChamber 展厅产品管理系统 · 纯本地离线运行</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { currentTheme, toggleTheme } from '../theme'

const router = useRouter()
const isDark = ref(false)

onMounted(() => {
  isDark.value = currentTheme() === 'dark'
})

function toggle() {
  isDark.value = toggleTheme() === 'dark'
}

const menu = [
  { label: '产品浏览', icon: 'pi pi-home', command: () => router.push('/') },
  { label: '产品管理', icon: 'pi pi-database', command: () => router.push('/manage') },
  { label: '批量导入', icon: 'pi pi-upload', command: () => router.push('/import') },
  { label: '二维码中心', icon: 'pi pi-qrcode', command: () => router.push('/qr') },
  { label: '意向单', icon: 'pi pi-shopping-cart', command: () => router.push('/intent-orders') },
  { label: '系统设置', icon: 'pi pi-cog', command: () => router.push('/settings') }
]
</script>
