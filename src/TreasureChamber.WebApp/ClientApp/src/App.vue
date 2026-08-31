<template>
  <div>
    <template v-if="route.meta.bare">
      <router-view />
    </template>
    <template v-else>
      <AppLayout />
    </template>
    <Toast position="top-right" />
    <ConfirmDialog />
  </div>
</template>

<script setup>
import { useRoute } from 'vue-router'
import AppLayout from './layout/AppLayout.vue'

const route = useRoute()
</script>
