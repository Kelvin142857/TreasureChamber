<template>
  <div>
    <!-- 搜索 Hero -->
    <div class="hero mb-4">
      <h1 class="fw-bold mb-2">珍宝展厅 · 产品目录</h1>
      <p class="mb-3 opacity-75">按系列 / 分类一层层浏览，输入型号或名称快速查找</p>
      <div class="d-flex gap-2" style="max-width: 560px;">
        <InputText v-model="keyword" placeholder="输入型号或名称，例如 XD-1001" class="w-full" @keyup.enter="search" />
        <Button label="搜索" icon="pi pi-search" @click="search" />
      </div>
    </div>

    <ProgressSpinner v-if="loading" style="width:48px;height:48px" class="d-block mx-auto my-5" />
    <template v-else>
      <!-- 按系列浏览 -->
      <h5 class="fw-bold mb-3">按系列浏览</h5>
      <div class="grid">
        <div v-for="s in catalog.series" :key="s.id" class="col-6 md:col-4 lg:col-3">
          <Card class="series-card" @click="goSeries(s.id)">
            <template #content>
              <div class="series-name">{{ s.name }}</div>
              <div class="text-muted small">{{ s.count }} 款产品</div>
            </template>
          </Card>
        </div>
      </div>

      <!-- 按分类浏览 -->
      <h5 class="fw-bold mt-4 mb-3">按分类浏览</h5>
      <div class="grid">
        <div class="col-12 md:col-6 lg:col-5">
          <Card>
            <template #content>
              <Tree :value="treeData" selection-mode="single" @node-select="onCatSelect" class="w-full" />
            </template>
          </Card>
        </div>
      </div>

      <!-- 最新上架 -->
      <h5 class="fw-bold mt-4 mb-3">最新上架</h5>
      <div v-if="catalog.recent.length" class="grid">
        <div v-for="p in catalog.recent" :key="p.id" class="col-6 md:col-4 lg:col-3">
          <Card class="cursor-pointer h-full" @click="goProduct(p.id)">
            <template #content>
              <div class="product-thumb mb-2">
                <img v-if="p.imagePath" :src="'/' + p.imagePath" :alt="p.name" loading="lazy" />
                <span v-else class="empty">暂无图片</span>
              </div>
              <div class="fw-semibold">{{ p.name }}</div>
              <div class="text-primary fw-bold small">{{ p.model }}</div>
              <div class="mt-2">
                <Tag v-if="p.seriesName" :value="p.seriesName" severity="secondary" class="me-1" />
                <Tag v-if="p.categoryName" :value="p.categoryName" severity="secondary" />
              </div>
            </template>
          </Card>
        </div>
      </div>
      <div v-else class="text-muted">暂无产品，请先到「批量导入」上传产品目录。</div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '../api'

const router = useRouter()
const keyword = ref('')
const loading = ref(true)
const catalog = ref({ series: [], categoryTree: [], recent: [] })

const treeData = ref([])

function flattenTree(nodes) {
  return nodes.map(n => ({
    key: String(n.id),
    label: `${n.name} (${n.count})`,
    data: n,
    children: flattenTree(n.children || [])
  }))
}

onMounted(async () => {
  try {
    const { data } = await api.get('/catalog')
    catalog.value = data
    treeData.value = flattenTree(data.categoryTree)
  } finally {
    loading.value = false
  }
})

function search() {
  router.push({ path: '/products', query: { keyword: keyword.value } })
}
function goSeries(id) {
  router.push({ path: '/products', query: { seriesId: id } })
}
function onCatSelect(node) {
  router.push({ path: '/products', query: { categoryId: node.data.id } })
}
function goProduct(id) {
  router.push(`/product/${id}`)
}
</script>
