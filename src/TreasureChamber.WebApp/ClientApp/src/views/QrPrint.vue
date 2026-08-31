<template>
  <div>
    <div v-if="labels.length" class="d-flex flex-wrap justify-content-center">
      <div v-for="p in labels" :key="p.id" class="qr-label">
        <img :src="`/api/products/${p.id}/qr`" alt="二维码" />
        <div class="model">{{ p.model }}</div>
        <div class="name">{{ p.name }}</div>
      </div>
    </div>
    <div v-else class="text-center py-5 text-muted">未选择产品</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../api'

const route = useRoute()
const router = useRouter()
const labels = ref([])

onMounted(async () => {
  const ids = route.query.ids
  if (!ids) return
  const { data } = await api.get('/qr/print', { params: { ids } })
  labels.value = data
  setTimeout(() => window.print(), 500)
})
</script>
