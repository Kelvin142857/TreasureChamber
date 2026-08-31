<template>
  <div class="grid">
    <!-- 侧栏：分类树 + 系列 -->
    <div class="col-12 lg:col-3">
      <Card class="mb-3">
        <template #title class="small">分类</template>
        <template #content>
          <div class="sidebar-cat">
            <Tree v-if="treeData.length" :value="treeData" selection-mode="single"
                  :selection-keys="catSelection" @node-select="onCatSelect" class="w-full" />
            <div v-else class="text-muted small">暂无分类</div>
          </div>
        </template>
      </Card>
      <Card>
        <template #title>系列</template>
        <template #content>
          <ul class="list-none p-0 m-0">
            <li class="mb-1">
              <a class="cat-leaf cursor-pointer text-color" :class="!query.seriesId ? 'text-primary font-bold' : ''" @click="selectSeries(null)">全部系列</a>
            </li>
            <li v-for="s in series" :key="s.id" class="mb-1">
              <a class="cat-leaf cursor-pointer" :class="query.seriesId === s.id ? 'text-primary font-bold' : 'text-color'"
                 @click="selectSeries(s.id)">{{ s.name }}</a>
            </li>
          </ul>
        </template>
      </Card>
    </div>

    <!-- 主区 -->
    <div class="col-12 lg:col-9">
      <div class="d-flex gap-2 mb-3">
        <InputText v-model="query.keyword" placeholder="搜索型号 / 名称" class="flex-1" @keyup.enter="load(1)" />
        <Button label="搜索" icon="pi pi-search" @click="load(1)" />
        <Button label="重置" severity="secondary" outlined @click="reset" />
      </div>
      <div class="text-muted small mb-2">共 {{ paged.total }} 款产品</div>

      <ProgressSpinner v-if="loading" style="width:48px;height:48px" class="d-block mx-auto my-5" />
      <template v-else>
        <div v-if="paged.items.length" class="grid">
          <div v-for="p in paged.items" :key="p.id" class="col-6 md:col-4 xl:col-3">
            <Card class="cursor-pointer h-full" @click="goProduct(p.id)">
              <template #content>
                <div class="product-thumb mb-2">
                  <img v-if="p.imagePath" :src="'/' + p.imagePath" :alt="p.name" loading="lazy" />
                  <span v-else class="empty">暂无图片</span>
                </div>
                <div class="fw-semibold small">{{ p.name }}</div>
                <div class="text-primary fw-bold small">{{ p.model }}</div>
                <div class="mt-2">
                  <Tag v-if="p.seriesName" :value="p.seriesName" severity="secondary" class="me-1" />
                  <Tag v-if="p.categoryName" :value="p.categoryName" severity="secondary" />
                </div>
              </template>
            </Card>
          </div>
        </div>
        <div v-else class="text-muted py-5 text-center">没有找到匹配的产品</div>

        <Paginator v-if="paged.totalPages > 1" :rows="pageSize" :totalRecords="paged.total"
                   :first="(paged.page - 1) * pageSize" @page="onPage" class="mt-3" />
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import api from '../api'

const router = useRouter()
const route = useRoute()
const pageSize = 12

const query = reactive({ seriesId: null, categoryId: null, keyword: '', page: 1 })
const paged = ref({ items: [], total: 0, page: 1, pageSize, totalPages: 0 })
const series = ref([])
const treeData = ref([])
const catSelection = ref({})
const loading = ref(true)

function flattenTree(nodes) {
  return nodes.map(n => ({
    key: String(n.id),
    label: `${n.name} (${n.count})`,
    data: n,
    children: flattenTree(n.children || [])
  }))
}

onMounted(async () => {
  query.seriesId = route.query.seriesId ? Number(route.query.seriesId) : null
  query.categoryId = route.query.categoryId ? Number(route.query.categoryId) : null
  query.keyword = route.query.keyword || ''
  const [{ data: cats }, { data: seriesData }] = await Promise.all([
    api.get('/catalog'),
    api.get('/catalog')
  ])
  treeData.value = flattenTree(cats.categoryTree)
  series.value = seriesData.series
  if (query.categoryId) catSelection.value = { [String(query.categoryId)]: true }
  load(1)
})

async function load(page) {
  loading.value = true
  try {
    const { data } = await api.get('/products', { params: { ...query, page, pageSize } })
    paged.value = data
  } finally {
    loading.value = false
  }
}

function onPage(e) { load(e.page + 1) }
function selectSeries(id) {
  query.seriesId = id
  load(1)
}
function onCatSelect(node) {
  query.categoryId = node.data.id
  catSelection.value = { [node.key]: true }
  load(1)
}
function reset() {
  Object.assign(query, { seriesId: null, categoryId: null, keyword: '', page: 1 })
  catSelection.value = {}
  load(1)
}
function goProduct(id) { router.push(`/product/${id}`) }
</script>
