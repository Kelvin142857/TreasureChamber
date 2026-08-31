<template>
  <div>
    <Breadcrumb :home="{ icon: 'pi pi-home', command: goHome }" :model="breadcrumb" class="mb-3" />

    <ProgressSpinner v-if="loading" style="width:48px;height:48px" class="d-block mx-auto my-5" />
    <template v-else-if="product">
      <div class="grid">
        <!-- 图片区 -->
        <div class="col-12 md:col-6">
          <Galleria v-if="product.images.length" :value="product.images" :numVisible="4" class="w-full">
            <template #item="slotProps">
              <img :src="'/' + slotProps.item.path" class="w-full galleria-img" :alt="product.name" />
            </template>
            <template #thumbnail="slotProps">
              <img :src="'/' + slotProps.item.path" class="w-full" style="height:64px;object-fit:cover" alt="" />
            </template>
          </Galleria>
          <div v-else class="product-thumb rounded border">
            <span class="empty">暂无图片</span>
          </div>
        </div>

        <!-- 信息区 -->
        <div class="col-12 md:col-6">
          <h2 class="fw-bold mb-1">{{ product.name }}</h2>
          <div class="text-primary fs-5 mb-2">型号：{{ product.model }}</div>
          <div class="mb-3">
            <Tag v-if="product.seriesName" :value="'系列：' + product.seriesName" severity="secondary" class="me-1" />
            <Tag v-if="product.categoryName" :value="'分类：' + product.categoryName" severity="secondary" />
          </div>
          <div v-if="product.description" class="mb-3">
            <h6 class="fw-bold">产品介绍</h6>
            <p class="text-muted" style="white-space:pre-wrap">{{ product.description }}</p>
          </div>

          <div class="d-flex align-items-center gap-3 mt-3">
            <img :src="`/api/products/${product.id}/qr`" width="120" height="120" alt="二维码" class="border rounded p-1" />
            <div class="small text-muted">
              <div class="fw-bold text-dark mb-1">扫码直达本产品</div>
              打印贴在产品旁边，客户手机扫码即可查看详情。
              <div class="mt-2">
                <Button label="下载二维码" icon="pi pi-download" size="small" outlined
                        @click="downloadQr" class="mr-2" />
                <Button label="加入意向单" icon="pi pi-shopping-cart" size="small" @click="addToIntentOrder" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 规格参数 -->
      <h5 v-if="product.specs.length" class="fw-bold mt-4 mb-2">规格参数</h5>
      <table v-if="product.specs.length" class="table table-bordered spec-table">
        <tbody>
          <tr v-for="spec in product.specs" :key="spec.id">
            <th>{{ spec.name }}</th>
            <td>{{ spec.value }}</td>
          </tr>
        </tbody>
      </table>

      <div class="mt-3">
        <Button label="返回列表" icon="pi pi-arrow-left" severity="secondary" outlined @click="goBack" />
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api, { downloadBlob } from '../api'

const route = useRoute()
const router = useRouter()
const product = ref(null)
const loading = ref(true)
const breadcrumb = ref([])

onMounted(async () => {
  try {
    const { data } = await api.get(`/products/${route.params.id}`)
    product.value = data
    breadcrumb.value = [
      { label: data.seriesName || '产品', command: () => router.push({ path: '/products', query: { seriesId: data.seriesId } }) },
      { label: data.model }
    ]
  } finally {
    loading.value = false
  }
})

function goHome() { router.push('/') }
function goBack() { router.back() }

async function downloadQr() {
  const res = await api.get(`/products/${product.value.id}/qr`, { responseType: 'blob' })
  downloadBlob(res.data, `${product.value.model}.png`, 'image/png')
}
function addToIntentOrder() {
  router.push({ path: '/intent-orders/new', query: { product: product.value.id } })
}
</script>
